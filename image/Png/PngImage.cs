using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Image.Png;

public class PngImage : IImageCanvas
{
    public static readonly byte[] MagicNumber = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required IColor[][] Canvas { get; init; }

    public static readonly (int XFactor, int YFactor, int XOffset, int YOffset)[] Interlaces = [
        (8, 8, 0, 0),
        (8, 8, 4, 0),
        (4, 8, 0, 4),
        (4, 4, 2, 0),
        (2, 4, 0, 2),
        (2, 2, 1, 0),
        (1, 2, 0, 1),
    ];

    public static PngImage FromStream(Stream stream)
    {
        var signature = stream.ReadExactly(8);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        var width = 0;
        var height = 0;
        var bit_deps = (byte)0;
        var color_type = ColorTypes.Grayscale;
        var compression_method = (byte)0;
        var filter_method = (byte)0;
        var interlace_method = (byte)0;
        IColor[] palette = [];
        using var memory = new MemoryStream();

        while (true)
        {
            var chunk = stream.ReadExactly(8).AsSpan();
            var length = BinaryPrimitives.ReadInt32BigEndian(chunk[0..4]);
            var type = (ChunkTypes)BinaryPrimitives.ReadUInt32BigEndian(chunk[4..8]);
            var chunkdataraw = stream.ReadExactly(length);
            var chunkdata = chunkdataraw.AsSpan();
            stream.Position += 4;
            switch (type)
            {
                case ChunkTypes.IHDR:
                    Debug.Assert(length == 13);
                    width = BinaryPrimitives.ReadInt32BigEndian(chunkdata[0..4]);
                    height = BinaryPrimitives.ReadInt32BigEndian(chunkdata[4..8]);
                    bit_deps = chunkdata[8];
                    color_type = (ColorTypes)chunkdata[9];
                    compression_method = chunkdata[10];
                    filter_method = chunkdata[11];
                    interlace_method = chunkdata[12];
                    break;

                case ChunkTypes.PLTE:
                    Debug.Assert(length >= 3 && length % 3 == 0);
                    palette = [.. Lists.RangeTo(0, (length / 3) - 1).Select(x => Color8.FromRgb(chunkdataraw[x * 3], chunkdataraw[(x * 3) + 1], chunkdataraw[(x * 3) + 2]))];
                    break;

                case ChunkTypes.IDAT:
                    memory.Write(chunkdataraw);
                    break;

                case ChunkTypes.IEND:
                    goto END_OF_DATA;

                case ChunkTypes.cHRM:
                case ChunkTypes.gAMA:
                case ChunkTypes.iCCP:
                case ChunkTypes.sBIT:
                case ChunkTypes.sRGB:
                case ChunkTypes.bKGD:
                case ChunkTypes.hIST:
                case ChunkTypes.tRNS:
                case ChunkTypes.pHYs:
                case ChunkTypes.sPLT:
                case ChunkTypes.tIME:
                case ChunkTypes.iTXt:
                case ChunkTypes.tEXt:
                case ChunkTypes.zTXt:
                    break;
            }
        }

    END_OF_DATA:

        memory.Position = 0;
        using var zlib = new ZLibStream(memory, CompressionMode.Decompress, true);
        var data = zlib.EnumerableReadBytes().ToArray();
        var bit_per_pixel = GetBitsPerPixel(color_type, bit_deps);
        var byte_per_pixel = BitToByte(bit_per_pixel);
        var row_byte = BitToByte(bit_per_pixel * width);

        if (interlace_method == 0)
        {
            ApplyFilterType(data, height, byte_per_pixel, row_byte + 1);
        }
        else
        {
            data = Deinterlacing(data, width, height, bit_per_pixel, byte_per_pixel, row_byte);
        }

        var makecolor = MakeColor(color_type, bit_deps, palette);
        var skip_filter_type = interlace_method == 0 ? 1 : 0;
        return new()
        {
            Width = width,
            Height = height,
            Canvas = [.. data
                .Chunk(row_byte + skip_filter_type)
                .Select(xs => (
                        color_type == ColorTypes.Grayscale && bit_deps < 8 ? ChunkBits(xs.Skip(skip_filter_type), bit_deps) :
                        color_type == ColorTypes.Palette && bit_deps < 8 ? ChunkBits(xs.Skip(skip_filter_type), bit_deps) :
                        xs.Skip(skip_filter_type).Chunk(byte_per_pixel)
                    )
                    .Take(width)
                    .Select(makecolor)
                    .ToArray()
                )],
        };
    }

    public static byte[] Deinterlacing(byte[] data, int width, int height, int bit_per_pixel, int byte_per_pixel, int row_byte)
    {
        var deinterlacing = new byte[row_byte * height];
        var offset = 0;
        var packed_bit_per_byte = bit_per_pixel < 8 ? 8 / bit_per_pixel : 0;
        var bit_mask = bit_per_pixel switch
        {
            1 => 0b1000_0000,
            2 => 0b1100_0000,
            4 => 0b1111_0000,
            _ => 0,
        };
        for (var pass = 0; pass < Interlaces.Length; pass++)
        {
            var p = Interlaces[pass];
            var pass_width = (width - p.XOffset + p.XFactor - 1) / p.XFactor;
            if (pass_width <= 0) continue;
            var pass_height = (height - p.YOffset + p.YFactor - 1) / p.YFactor;
            if (pass_height <= 0) continue;
            var pass_row_byte = BitToByte(bit_per_pixel * pass_width);
            var pass_span = data.AsSpan(offset, (pass_row_byte * pass_height) + pass_height);

            ApplyFilterType(pass_span, pass_height, byte_per_pixel, pass_row_byte + 1);
            offset += pass_span.Length;

            var pass_index = 1;
            if (bit_per_pixel < 8)
            {
                for (var y = 0; y < pass_height; y++)
                {
                    var deinterlacing_scanline_offset = ((y * p.YFactor) + p.YOffset) * row_byte;
                    for (var x = 0; x < pass_width; x++)
                    {
                        var x_offset = p.XOffset + (x * p.XFactor);
                        var deinterlacing_offset = deinterlacing_scanline_offset + (x_offset / packed_bit_per_byte);
                        var x_shift = x_offset % packed_bit_per_byte * bit_per_pixel;
                        var x_bit_offset = x % packed_bit_per_byte * bit_per_pixel;
                        var x_bit = (pass_span[pass_index] & (bit_mask >> x_bit_offset)) << x_bit_offset;
                        deinterlacing[deinterlacing_offset] |= (byte)(x_bit >> x_shift);
                        if (x % packed_bit_per_byte == packed_bit_per_byte - 1 || x + 1 == pass_width) pass_index++;
                    }
                    pass_index++;
                }
            }
            else
            {
                for (var y = 0; y < pass_height; y++)
                {
                    var deinterlacing_scanline_offset = (((y * p.YFactor) + p.YOffset) * row_byte) + (p.XOffset * byte_per_pixel);
                    for (var x = 0; x < pass_width; x++)
                    {
                        var deinterlacing_offset = deinterlacing_scanline_offset + (x * p.XFactor * byte_per_pixel);
                        for (var b = 0; b < byte_per_pixel; b++)
                        {
                            deinterlacing[deinterlacing_offset + b] = pass_span[pass_index++];
                        }
                    }
                    pass_index++;
                }
            }
        }
        return deinterlacing;
    }

    public static int GetBitsPerPixel(ColorTypes color_type, byte bit_deps) => color_type switch
    {
        ColorTypes.Grayscale => bit_deps,
        ColorTypes.Rgb => bit_deps * 3,
        ColorTypes.Palette => bit_deps,
        ColorTypes.GrayscaleAlpha => bit_deps * 2,
        ColorTypes.Rgba => bit_deps * 4,
        _ => throw new(),
    };

    public static int BitToByte(int bit) => (bit + 7) / 8;

    public static IEnumerable<byte[]> ChunkBits(IEnumerable<byte> self, int bit_deps)
    {
        foreach (var b in self)
        {
            switch (bit_deps)
            {
                case 1:
                    yield return [(byte)((b & 0x80) >> 7)];
                    yield return [(byte)((b & 0x40) >> 6)];
                    yield return [(byte)((b & 0x20) >> 5)];
                    yield return [(byte)((b & 0x10) >> 4)];
                    yield return [(byte)((b & 0x08) >> 3)];
                    yield return [(byte)((b & 0x04) >> 2)];
                    yield return [(byte)((b & 0x02) >> 1)];
                    yield return [(byte)(b & 0x01)];
                    break;

                case 2:
                    yield return [(byte)((b & 0xC0) >> 6)];
                    yield return [(byte)((b & 0x30) >> 4)];
                    yield return [(byte)((b & 0x0C) >> 2)];
                    yield return [(byte)(b & 0x03)];
                    break;

                case 4:
                    yield return [(byte)((b & 0xF0) >> 4)];
                    yield return [(byte)(b & 0x0F)];
                    break;
            }
        }
    }

    public static void ApplyFilterType(Span<byte> datas, int height, int byte_per_pixel, int row_byte_with_filter_type)
    {
        ReadOnlySpan<byte> prev_scanline = stackalloc byte[row_byte_with_filter_type - 1];
        for (var y = 0; y < height; y++)
        {
            var line = datas[(y * row_byte_with_filter_type)..((y * row_byte_with_filter_type) + row_byte_with_filter_type)];
            var filter_type = (FilterTypes)line[0];
            var scanline = line[1..];

            switch (filter_type)
            {
                case FilterTypes.None: break;

                case FilterTypes.Sub:
                    for (var x = byte_per_pixel; x < scanline.Length; x++)
                    {
                        scanline[x] += scanline[x - byte_per_pixel];
                    }
                    break;

                case FilterTypes.Up:
                    for (var x = 0; x < scanline.Length; x++)
                    {
                        scanline[x] += prev_scanline[x];
                    }
                    break;

                case FilterTypes.Average:
                    for (var x = 0; x < byte_per_pixel; x++)
                    {
                        scanline[x] += (byte)(prev_scanline[x] / 2);
                    }
                    for (var x = byte_per_pixel; x < scanline.Length; x++)
                    {
                        scanline[x] += (byte)((scanline[x - byte_per_pixel] + prev_scanline[x]) / 2);
                    }
                    break;

                case FilterTypes.Paeth:
                    for (var i = 0; i < byte_per_pixel; i++)
                    {
                        var a = 0;
                        var c = 0;
                        for (var x = i; x < scanline.Length; x += byte_per_pixel)
                        {
                            var b = (int)prev_scanline[x];
                            var pa = Math.Abs(b - c);
                            var pb = Math.Abs(a - c);
                            var pc = Math.Abs(a + b - c - c);

                            var paeth_predictor = (pa <= pb && pa <= pc) ? a : (pb <= pc ? b : c);
                            a = scanline[x] = (byte)(scanline[x] + paeth_predictor);
                            c = b;
                        }
                    }
                    break;
            }
            prev_scanline = scanline;
        }
    }

    public static Func<byte[], IColor> MakeColor(ColorTypes color_type, int bit_deps, IColor[] palette) =>
        color_type == ColorTypes.Grayscale && bit_deps == 1 ? xs => Color8.FromRgb(xs[0] * 255, xs[0] * 255, xs[0] * 255) :
        color_type == ColorTypes.Grayscale && bit_deps == 2 ? xs => Color8.FromRgb(xs[0] * 85, xs[0] * 85, xs[0] * 85) :
        color_type == ColorTypes.Grayscale && bit_deps == 4 ? xs => Color8.FromRgb(xs[0] * 17, xs[0] * 17, xs[0] * 17) :
        color_type == ColorTypes.Grayscale && bit_deps == 8 ? xs => Color8.FromRgb(xs[0], xs[0], xs[0]) :
        color_type == ColorTypes.Grayscale && bit_deps == 16 ? xs => Color16.FromRgb((xs[0] << 8) | xs[1], (xs[0] << 8) | xs[1], (xs[0] << 8) | xs[1]) :
        color_type == ColorTypes.Rgb && bit_deps == 8 ? xs => Color8.FromRgb(xs[0], xs[1], xs[2]) :
        color_type == ColorTypes.Rgb && bit_deps == 16 ? xs => Color16.FromRgb((xs[0] << 8) | xs[1], (xs[2] << 8) | xs[3], (xs[4] << 8) | xs[5]) :
        color_type == ColorTypes.Palette ? xs => palette[xs[0]] : // bit_per_pixel is 1, 2, 4 to 1-bytes array.
        color_type == ColorTypes.GrayscaleAlpha && bit_deps == 8 ? xs => Color8.FromArgb(xs[1], xs[0], xs[0], xs[0]) :
        color_type == ColorTypes.GrayscaleAlpha && bit_deps == 16 ? xs => Color16.FromArgb((xs[2] << 8) | xs[3], (xs[0] << 8) | xs[1], (xs[0] << 8) | xs[1], (xs[0] << 8) | xs[1]) :
        color_type == ColorTypes.Rgba && bit_deps == 8 ? xs => Color8.FromArgb(xs[3], xs[0], xs[1], xs[2]) :
        color_type == ColorTypes.Rgba && bit_deps == 16 ? xs => Color16.FromArgb((xs[6] << 8) | xs[7], (xs[0] << 8) | xs[1], (xs[2] << 8) | xs[3], (xs[4] << 8) | xs[5]) :
        throw new InvalidOperationException($"Unsupported color-type: {color_type}, bit-depth: {bit_deps}.");
}

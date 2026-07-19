using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Image.Png;

public class PngFile : IImageCanvas
{
    public static readonly byte[] MagicNumber = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color[][] Canvas { get; init; }

    public static PngFile FromStream(Stream stream)
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
        Color[] palette = [];
        var datas = new List<byte>();

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
                    palette = [.. Lists.RangeTo(0, (length / 3) - 1).Select(x => Color.FromArgb(chunkdataraw[x * 3], chunkdataraw[(x * 3) + 1], chunkdataraw[(x * 3) + 2]))];
                    break;

                case ChunkTypes.IDAT:
                    datas.AddRange(chunkdataraw);
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

        using var memory = new MemoryStream([.. datas]);
        using var zlib = new ZLibStream(memory, CompressionMode.Decompress, true);
        var data = zlib.EnumerableReadBytes().ToArray();
        var bit_per_pixel = GetBitsPerPixel(color_type, bit_deps);
        var byte_per_pixel = BitToByte(bit_per_pixel);
        var row_byte = 1 + BitToByte(bit_per_pixel * width);
        ApplyFilterType(data, height, byte_per_pixel, row_byte);

        Func<byte[], Color> makecolor =
            color_type == ColorTypes.Palette && bit_per_pixel == 16 ? xs => palette[(xs[0] << 8) + xs[1]] :
            color_type == ColorTypes.Palette && bit_per_pixel == 8 ? xs => palette[xs[0]] :
            color_type == ColorTypes.Grayscale && bit_per_pixel == 16 ? xs => Color.FromArgb(xs[0], xs[0], xs[0]) :
            color_type == ColorTypes.Grayscale && bit_per_pixel == 8 ? xs => Color.FromArgb(xs[0], xs[0], xs[0]) :
            color_type == ColorTypes.Grayscale && bit_per_pixel == 4 ? xs => Color.FromArgb(xs[0] * 17, xs[0] * 17, xs[0] * 17) :
            color_type == ColorTypes.Grayscale && bit_per_pixel == 2 ? xs => Color.FromArgb(xs[0] * 85, xs[0] * 85, xs[0] * 85) :
            color_type == ColorTypes.Grayscale && bit_per_pixel == 1 ? xs => Color.FromArgb(xs[0] * 255, xs[0] * 255, xs[0] * 255) :
            byte_per_pixel == 2 ? xs => throw new NotSupportedException() :
            byte_per_pixel == 3 ? xs => Color.FromArgb(xs[0], xs[1], xs[2]) :
            byte_per_pixel == 4 ? xs => Color.FromArgb(xs[3], xs[0], xs[1], xs[2]) :
            xs => Color.FromArgb(xs[0], xs[0], xs[0]);

        return new()
        {
            Width = width,
            Height = height,
            Canvas = [.. data
                .Chunk(row_byte)
                .Select(xs => (color_type == ColorTypes.Grayscale && bit_per_pixel < 8 ?
                        ChunkBits(xs.Skip(1), bit_per_pixel) :
                        xs.Skip(1).Chunk(byte_per_pixel)
                    )
                    .Select(makecolor)
                    .ToArray()
                )],
        };
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

    public static IEnumerable<byte[]> ChunkBits(IEnumerable<byte> self, int bit)
    {
        foreach (var b in self)
        {
            switch (bit)
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

    public static void ApplyFilterType(Span<byte> datas, int height, int byte_per_pixel, int row_byte)
    {
        ReadOnlySpan<byte> prev_scanline = stackalloc byte[row_byte - 1];
        for (var y = 0; y < height; y++)
        {
            var line = datas[(y * row_byte)..((y * row_byte) + row_byte)];
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
}

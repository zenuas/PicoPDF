using Extensions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PicoPDF.Image.Png;

public class PngFile : IImageCanvas
{
    public static readonly byte[] MagicNumber = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color[][] Canvas { get; init; }

    public static PngFile FromStream(Stream stream)
    {
        var signature = stream.ReadBytes(8);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        var width = 0;
        var height = 0;
        var bit_deps = (byte)0;
        var color_type = (byte)0;
        var compression_method = (byte)0;
        var filter_method = (byte)0;
        var interlace_method = (byte)0;
        var palette = new List<Color>();
        var datas = new List<byte>();

        while (true)
        {
            var chunk = stream.ReadBytes(8).AsSpan();
            if (chunk.Length < 8) break;

            var length = BinaryPrimitives.ReadInt32BigEndian(chunk[0..4]);
            var type = BinaryPrimitives.ReadUInt32BigEndian(chunk[4..8]);
            var chunkdataraw = stream.ReadBytes(length);
            var chunkdata = chunkdataraw.AsSpan();
            stream.Position += 4;
            switch (type)
            {
                case (uint)ChunkTypes.IHDR:
                    Debug.Assert(length == 13);
                    width = BinaryPrimitives.ReadInt32BigEndian(chunkdata[0..4]);
                    height = BinaryPrimitives.ReadInt32BigEndian(chunkdata[4..8]);
                    bit_deps = chunkdata[8];
                    color_type = chunkdata[9];
                    compression_method = chunkdata[10];
                    filter_method = chunkdata[11];
                    interlace_method = chunkdata[12];
                    break;

                case (uint)ChunkTypes.PLTE:
                    Debug.Assert(length >= 3 && length % 3 == 0);
                    Lists.RangeTo(0, (length / 3) - 1).Select(x => Color.FromArgb(chunkdataraw[x * 3], chunkdataraw[(x * 3) + 1], chunkdataraw[(x * 3) + 2])).Each(palette.Add);
                    break;

                case (uint)ChunkTypes.IDAT:
                    datas.AddRange(chunkdataraw);
                    break;

                case (uint)ChunkTypes.IEND:
                    goto END_OF_DATA;

                case (uint)ChunkTypes.cHRM:
                case (uint)ChunkTypes.gAMA:
                case (uint)ChunkTypes.iCCP:
                case (uint)ChunkTypes.sBIT:
                case (uint)ChunkTypes.sRGB:
                case (uint)ChunkTypes.bKGD:
                case (uint)ChunkTypes.hIST:
                case (uint)ChunkTypes.tRNS:
                case (uint)ChunkTypes.pHYs:
                case (uint)ChunkTypes.sPLT:
                case (uint)ChunkTypes.tIME:
                case (uint)ChunkTypes.iTXt:
                case (uint)ChunkTypes.tEXt:
                case (uint)ChunkTypes.zTXt:
                    break;
            }
        }

    END_OF_DATA:

        using var memory = new MemoryStream(datas.ToArray());
        using var zlib = new ZLibStream(memory, CompressionMode.Decompress, true);
        var data = zlib.ReadAllBytes().ToArray();
        var bit_per_pixel = GetBitsPerPixel(color_type, bit_deps);
        var byte_per_pixel = BitToByte(bit_per_pixel);
        var row_byte = 1 + BitToByte(bit_per_pixel * width);
        ApplyFilterType(data, height, byte_per_pixel, row_byte);

        Func<byte[], Color> makecolor =
            color_type == 3 ? xs => palette[xs[0]] :
            byte_per_pixel == 3 ? xs => Color.FromArgb(xs[0], xs[1], xs[2]) :
            byte_per_pixel == 4 ? xs => Color.FromArgb(xs[3], xs[0], xs[1], xs[2]) :
            byte_per_pixel == 1 ? xs => Color.FromArgb(xs[0], xs[0], xs[0]) :
            xs => Color.FromArgb(xs[0], xs[0], xs[0]);

        return new PngFile()
        {
            Width = width,
            Height = height,
            Canvas = data
                .Chunk(row_byte)
                .Select(xs => xs.Skip(1).Chunk(byte_per_pixel).Select(makecolor).ToArray())
                .ToArray(),
        };
    }

    public static int GetBitsPerPixel(byte color_type, byte bit_deps) => color_type switch
    {
        (byte)ColorTypes.Grayscale => bit_deps,
        (byte)ColorTypes.Rgb => bit_deps * 3,
        (byte)ColorTypes.Palette => bit_deps,
        (byte)ColorTypes.GrayscaleAlpha => bit_deps * 2,
        (byte)ColorTypes.Rgba => bit_deps * 4,
        _ => throw new(),
    };

    public static int BitToByte(int bit) => (bit + 7) / 8;

    public static void ApplyFilterType(Span<byte> datas, int height, int byte_per_pixel, int row_byte)
    {
        var prev_scanline = new byte[row_byte - 1].AsSpan();
        for (var y = 0; y < height; y++)
        {
            var line = datas[(y * row_byte)..((y * row_byte) + row_byte)];
            var filter_type = line[0];
            var scanline = line[1..];

            switch (filter_type)
            {
                case (byte)FilterTypes.None: break;

                case (byte)FilterTypes.Sub:
                    for (var x = byte_per_pixel; x < scanline.Length; x++)
                    {
                        scanline[x] += scanline[x - byte_per_pixel];
                    }
                    break;

                case (byte)FilterTypes.Up:
                    for (var x = 0; x < scanline.Length; x++)
                    {
                        scanline[x] += prev_scanline[x];
                    }
                    break;

                case (byte)FilterTypes.Average:
                    for (var x = 0; x < byte_per_pixel; x++)
                    {
                        scanline[x] += (byte)(prev_scanline[x] / 2);
                    }
                    for (var x = byte_per_pixel; x < scanline.Length; x++)
                    {
                        scanline[x] += (byte)((scanline[x - byte_per_pixel] + prev_scanline[x]) / 2);
                    }
                    break;

                case (byte)FilterTypes.Paeth:
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

using Mina.Extension;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Image.Bmp;

public class BmpFile : IImageCanvas, IImageWritable
{
    public static readonly byte[] MagicNumber = [0x42, 0x4D];
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color[][] Canvas { get; init; }

    public void Write(Stream stream)
    {
        // BITMAPFILEHEADER
        stream.Write(MagicNumber);
        stream.WriteUIntByLittleEndian((uint)(/* sizeof(BITMAPFILEHEADER) */14 + /* sizeof(BITMAPINFOHEADER) */40 + (Width * Height * 4)));
        stream.WriteUShortByLittleEndian(0);
        stream.WriteUShortByLittleEndian(0);
        stream.WriteUIntByLittleEndian(/* sizeof(BITMAPFILEHEADER) */14 + /* sizeof(BITMAPINFOHEADER) */40);

        // BITMAPINFOHEADER
        stream.WriteUIntByLittleEndian(/* sizeof(BITMAPINFOHEADER) */40);
        stream.WriteIntByLittleEndian(Width);
        stream.WriteIntByLittleEndian(-Height);
        stream.WriteUShortByLittleEndian(1);
        stream.WriteUShortByLittleEndian(32);
        stream.WriteUIntByLittleEndian(0);
        stream.WriteUIntByLittleEndian(0);
        stream.WriteIntByLittleEndian(0);
        stream.WriteIntByLittleEndian(0);
        stream.WriteUIntByLittleEndian(0);
        stream.WriteUIntByLittleEndian(0);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var color = Canvas[y][x];
                var a = color.A / 255F;

                stream.WriteByte((byte)Math.Round((color.B * a) + (255 * (1 - a))));
                stream.WriteByte((byte)Math.Round((color.G * a) + (255 * (1 - a))));
                stream.WriteByte((byte)Math.Round((color.R * a) + (255 * (1 - a))));
                stream.WriteByte(0);
            }
        }
    }

    public static BmpFile FromStream(Stream stream)
    {
        var position = stream.Position;

        // BITMAPFILEHEADER
        var signature = stream.ReadExactly(2);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        stream.Position += 8;
        var bfOffBits = stream.ReadUIntByLittleEndian();

        var header_size = stream.ReadUIntByLittleEndian();
        var width = 0;
        var height = 0;
        var bitcount = 1;

        switch (header_size)
        {
            case 12: // BITMAPCOREHEADER
                width = stream.ReadUShortByLittleEndian();
                height = stream.ReadUShortByLittleEndian();
                _ = stream.ReadUShortByLittleEndian();
                bitcount = stream.ReadUShortByLittleEndian();
                break;

            case 40: // BITMAPINFOHEADER
                width = stream.ReadIntByLittleEndian();
                height = stream.ReadIntByLittleEndian();
                _ = stream.ReadUShortByLittleEndian();
                bitcount = stream.ReadUShortByLittleEndian();
                var compression = stream.ReadUIntByLittleEndian();
                if (compression != 0) throw new InvalidOperationException($"Unsupported bitmap compression({compression}).");
                stream.Position += 20;
                break;

            default:
                throw new InvalidOperationException($"Unsupported bitmap header size({header_size}).");
        }

        Color[] palettes = null!;
        switch (bitcount)
        {
            case 1:
            case 4:
            case 8:
                {
                    palettes = bitcount switch
                    {
                        1 => new Color[2],
                        4 => new Color[16],
                        8 => new Color[256],
                        _ => [],
                    };
                    var color_table_size = (bfOffBits - (14 + header_size)) / 4;
                    if (palettes.Length != color_table_size) throw new InvalidOperationException($"Invalid number of colors in {bitcount}-bit bitmap palette({color_table_size}).");
                    for (var i = 0; i < color_table_size; i++)
                    {
                        var b = stream.ReadByte();
                        var g = stream.ReadByte();
                        var r = stream.ReadByte();
                        _ = stream.ReadByte();
                        palettes[i] = Color.FromArgb(r, g, b);
                    }
                    break;
                }

            case 16:
            case 24:
            case 32:
                stream.Position = position + bfOffBits;
                break;

            default:
                throw new InvalidOperationException($"Unsupported bitmap bit-count({bitcount}).");
        }

        var height_abs = Math.Abs(height);
        var canvas = new Color[height_abs][];
        var stride = ((bitcount * width) + 31) / 32 * 4;
        var padding = stride - (bitcount * width / 8);
        for (var i = 0; i < height_abs; i++)
        {
            var row = new Color[width];
            var palette_data = 0;

            for (var x = 0; x < width; x++)
            {
                switch (bitcount)
                {
                    case 1:
                        if ((x % 8) == 0) palette_data = stream.ReadByte();
                        row[x] = palettes[(palette_data >> (7 - (x % 8))) & 0b00000001];
                        break;

                    case 4:
                        if ((x % 2) == 0) palette_data = stream.ReadByte();
                        row[x] = palettes[(palette_data >> (x % 2 * 4)) & 0b00001111];
                        break;

                    case 8:
                        row[x] = palettes[stream.ReadByte()];
                        break;

                    case 16:
                        {
                            var rgb = stream.ReadUShortByLittleEndian();
                            var r = (byte)((rgb & 0b01111100_00000000) >> 10);
                            var g = (byte)((rgb & 0b00000011_11100000) >> 5);
                            var b = (byte)(rgb & 0b00000000_00011111);
                            row[x] = Color.FromArgb(r, g, b);
                        }
                        break;

                    case 24:
                    case 32:
                        {
                            var b = stream.ReadByte();
                            var g = stream.ReadByte();
                            var r = stream.ReadByte();
                            if (bitcount == 32) _ = stream.ReadByte();
                            row[x] = Color.FromArgb(r, g, b);
                        }
                        break;
                }
            }
            stream.Position += padding;

            // For uncompressed RGB bitmaps,
            // if biHeight is positive, the bitmap is a bottom-up DIB with the origin at the lower left corner.
            // If biHeight is negative, the bitmap is a top-down DIB with the origin at the upper left corner.
            canvas[height > 0 ? height_abs - 1 - i : i] = row;
        }

        return new()
        {
            Width = width,
            Height = height_abs,
            Canvas = canvas,
        };
    }
}

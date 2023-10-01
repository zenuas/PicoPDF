using System;
using System.Drawing;
using System.IO;

namespace PicoPDF.Image.Bmp;

public class BmpFile : IImageCanvas, IImageWritable
{
    public static readonly byte[] MagicNumber = new byte[] { 0x42, 0x4D };
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color[][] Canvas { get; init; }

    public void Write(Stream stream)
    {
        // BITMAPFILEHEADER
        stream.Write(MagicNumber);
        stream.Write(BitConverter.GetBytes(((uint)(/* sizeof(BITMAPFILEHEADER) */14 + /* sizeof(BITMAPINFOHEADER) */40 + (Width * Height * 4)))));
        stream.Write(BitConverter.GetBytes((ushort)0));
        stream.Write(BitConverter.GetBytes((ushort)0));
        stream.Write(BitConverter.GetBytes((uint)/* sizeof(BITMAPFILEHEADER) */14 + /* sizeof(BITMAPINFOHEADER) */40));

        // BITMAPINFOHEADER
        stream.Write(BitConverter.GetBytes((uint)/* sizeof(BITMAPINFOHEADER) */40));
        stream.Write(BitConverter.GetBytes(Width));
        stream.Write(BitConverter.GetBytes(-Height));
        stream.Write(BitConverter.GetBytes((ushort)1));
        stream.Write(BitConverter.GetBytes((ushort)32));
        stream.Write(BitConverter.GetBytes((uint)0));
        stream.Write(BitConverter.GetBytes((uint)0));
        stream.Write(BitConverter.GetBytes((uint)0));
        stream.Write(BitConverter.GetBytes((uint)0));
        stream.Write(BitConverter.GetBytes((uint)0));
        stream.Write(BitConverter.GetBytes((uint)0));

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                stream.WriteByte(Canvas[y][x].B);
                stream.WriteByte(Canvas[y][x].G);
                stream.WriteByte(Canvas[y][x].R);
                stream.WriteByte(0);
            }
        }
    }
}

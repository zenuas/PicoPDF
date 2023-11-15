using Extensions;
using System.Drawing;
using System.IO;

namespace PicoPDF.Image.Bmp;

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
                stream.WriteByte(Canvas[y][x].B);
                stream.WriteByte(Canvas[y][x].G);
                stream.WriteByte(Canvas[y][x].R);
                stream.WriteByte(0);
            }
        }
    }
}

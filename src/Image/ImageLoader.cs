using Mina.Extension;
using PicoPDF.Image.Bmp;
using PicoPDF.Image.Jpeg;
using PicoPDF.Image.Png;
using System.IO;
using System.Linq;

namespace PicoPDF.Image;

public static class ImageLoader
{
    public static IImage? FromFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return FromStream(stream);
    }

    public static IImage? FromFile(string path, ImageTypes type)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return FromStream(stream, type);
    }

    public static IImage? FromStream(Stream stream)
    {
        var position = stream.Position;
        var type = TypeCheck(stream);
        stream.Position = position;

        return FromStream(stream, type);
    }

    public static IImage? FromStream(Stream stream, ImageTypes type)
    {
        switch (type)
        {
            case ImageTypes.Jpeg:
                return JpegFile.FromStream(stream);

            case ImageTypes.Png:
                return PngFile.FromStream(stream);
        }
        return null;
    }

    public static ImageTypes TypeCheck(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return TypeCheck(stream);
    }

    public static ImageTypes TypeCheck(Stream stream)
    {
        var position = stream.Position;
        var header = stream.ReadExactly(9);
        stream.Position = position;

        if (header.Length >= JpegFile.MagicNumber.Length && header.Take(JpegFile.MagicNumber.Length).SequenceEqual(JpegFile.MagicNumber)) return ImageTypes.Jpeg;
        if (header.Length >= PngFile.MagicNumber.Length && header.Take(PngFile.MagicNumber.Length).SequenceEqual(PngFile.MagicNumber)) return ImageTypes.Png;
        if (header.Length >= BmpFile.MagicNumber.Length && header.Take(BmpFile.MagicNumber.Length).SequenceEqual(BmpFile.MagicNumber)) return ImageTypes.Bmp;

        return ImageTypes.Unknown;
    }
}

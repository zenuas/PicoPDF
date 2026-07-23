using Image.Bmp;
using Image.Jpeg;
using Image.Png;
using Mina.Extension;
using System.IO;
using System.Linq;

namespace Image;

public static class ImageLoader
{
    public static IImage? FromFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return FromStream(stream);
    }

    public static IImage? FromFile(string path, ImageTypes type)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return FromStream(stream, type);
    }

    public static IImage? FromStream(Stream stream)
    {
        var position = stream.Position;
        var type = TypeCheck(stream);
        stream.Position = position;
        return FromStream(stream, type);
    }

    public static IImage? FromStream(Stream stream, ImageTypes type) => type switch
    {
        ImageTypes.Jpeg => JpegImage.FromStream(stream),
        ImageTypes.Png => PngImage.FromStream(stream),
        ImageTypes.Bmp => BmpImage.FromStream(stream),
        _ => null,
    };

    public static ImageTypes TypeCheck(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Using(TypeCheck);

    public static ImageTypes TypeCheck(Stream stream)
    {
        var header = stream.ReadExactly(9);

        return header.Length >= JpegImage.MagicNumber.Length && header.Take(JpegImage.MagicNumber.Length).SequenceEqual(JpegImage.MagicNumber) ? ImageTypes.Jpeg
            : header.Length >= PngImage.MagicNumber.Length && header.Take(PngImage.MagicNumber.Length).SequenceEqual(PngImage.MagicNumber) ? ImageTypes.Png
            : header.Length >= BmpImage.MagicNumber.Length && header.Take(BmpImage.MagicNumber.Length).SequenceEqual(BmpImage.MagicNumber) ? ImageTypes.Bmp
            : ImageTypes.Unknown;
    }
}

using System.IO;

namespace PicoPDF.Image.Bmp;

public class BmpFile : IImage
{
    public static readonly byte[] MagicNumber = new byte[] { 0x42, 0x4D };
    public required int Width { get; set; }
    public required int Height { get; set; }

    public static BmpFile FromStream(Stream stream)
    {
        return new BmpFile() { Width = 0, Height = 0 };
    }
}

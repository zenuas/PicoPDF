using Extensions;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.Image.Png;

public class PngFile : IImage
{
    public static readonly byte[] MagicNumber = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    public required int Width { get; init; }
    public required int Height { get; init; }

    public static PngFile FromStream(Stream stream)
    {
        var signature = stream.ReadBytes(8);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        return new PngFile() { Width = 0, Height = 0 };
    }
}

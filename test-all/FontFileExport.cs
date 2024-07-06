using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.LoadComplete(opt.FontFileExport);
        if (font is TrueTypeFont ttf) Export(ttf, opt);
    }

    public static void Export(TrueTypeFont font, Option opt)
    {
        using var stream = new FileStream(opt.OutputFontFile, FileMode.Create, FileAccess.Write, FileShare.None);
        FontExporter.Export(FontExtract.Extract(font,
            new()
            {
                ExtractChars = opt.FontExportChars.ToCharArray(),
                OutputNames = [
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FontFamilyName),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FontSubfamilyName),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.UniqueFontIdentifier),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FullFontName),
                ],
                OutputCMap = [
                    new(Platforms.Unicode, Encodings.Unicode2_0_BMPOnly),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP),
                ],
            }), stream);
    }
}

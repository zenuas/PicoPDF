using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.LoadComplete(fontreg.LoadRequiredTables(opt.FontFileExport, true));
        if (font is TrueTypeFont ttf) Export(Extract(ttf, opt), opt);
        else if (font is PostScriptFont otf) Export(Extract(otf, opt), opt);
    }

    public static void Export(IOpenTypeRequiredTables font, Option opt)
    {
        using var stream = new FileStream(opt.OutputFontFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        if (font is TrueTypeFont ttf) FontExporter.Export(ttf, stream);
        else if (font is PostScriptFont otf) FontExporter.Export(otf, stream);
    }

    public static TrueTypeFont Extract(TrueTypeFont font, Option opt) => FontExtract.Extract(font,
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
            });

    public static PostScriptFont Extract(PostScriptFont font, Option opt) => FontExtract.Extract(font,
            new()
            {
                ExtractChars = opt.FontExportChars.ToCharArray(),
                OutputNames = [
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FontFamilyName),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FontSubfamilyName),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.UniqueFontIdentifier),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.FullFontName),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.Version),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP, null, NameIDs.PostScriptName),
                ],
                OutputCMap = [
                    new(Platforms.Unicode, Encodings.Unicode2_0_BMPOnly),
                    new(Platforms.Windows, Encodings.Windows_UnicodeBMP),
                ],
            });
}

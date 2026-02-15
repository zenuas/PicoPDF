using Mina.Extension;
using PicoPDF.OpenType;
using System.IO;
using Encodings = PicoPDF.OpenType.Encodings;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(IOpenTypeRequiredTables font, Option opt)
    {
        using var stream = new FileStream(opt.OutputFontFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        if (font is TrueTypeFont ttf) FontExporter.Export(Extract(ttf, opt), stream);
        else if (font is PostScriptFont psf) FontExporter.Export(Extract(psf, opt), stream);
    }

    public static TrueTypeFont Extract(TrueTypeFont font, Option opt) => FontExtract.Extract(font,
            new()
            {
                ExtractChars = [.. opt.FontExportChars.ToUtf32CharArray()],
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
                ExtractChars = [.. opt.FontExportChars.ToUtf32CharArray()],
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

using Mina.Extension;
using OpenType;
using OpenType.Tables;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(IOpenTypeFont font, Option opt)
    {
        using var stream = new FileStream(opt.OutputFontFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        var extract_opt = CreateOption(opt);
        var fontextract = FontExtract.Extract(font, extract_opt);
        FontExporter.Export(fontextract, stream);
    }

    public static FontExtractOption CreateOption(Option opt) => new()
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
            new(Platforms.Unicode, Encodings.Unicode2_0_FullRepertoire, CMapFormats.Format12),
            new(Platforms.Windows, Encodings.Windows_UnicodeBMP, CMapFormats.Format4),
            new(Platforms.Windows, Encodings.Windows_UnicodeFullRepertoire, CMapFormats.Format12),
        ],
    };
}

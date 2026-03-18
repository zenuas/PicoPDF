using Mina.Extension;
using OpenType;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(IOpenTypeRequiredTables font, Option opt)
    {
        using var stream = new FileStream(opt.OutputFontFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        var extract_opt = CreateOption(opt);
        if (font is TrueTypeFont ttf) FontExporter.Export(FontExtract.Extract(ttf, extract_opt), stream);
        else if (font is PostScriptFont psf) FontExporter.Export(FontExtract.Extract(psf, extract_opt), stream);
        else if (font is NoOutlineFont noo) FontExporter.Export(FontExtract.Extract(noo, extract_opt), stream);
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

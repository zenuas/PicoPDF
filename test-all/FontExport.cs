using Mina.Command;
using Mina.Extension;
using OpenType;
using System.IO;

namespace PicoPDF.TestAll;

public class FontExport : FontRegisterCommand
{
    [CommandOption("export-chars")]
    public string ExportChars { get; init; } = "a";

    [CommandOption("output"), CommandOption('o')]
    public string Output { get; init; } = "";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        foreach (var arg in args)
        {
            var font = fontreg.LoadComplete(arg);
            Export(font, Output, CreateOption(ExportChars));
        }

    }

    public static void Export(IOpenTypeFont font, string output, FontExtractOption extract_opt)
    {
        using var stream = new FileStream(output, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        var fontextract = FontExtract.Extract(font, extract_opt);
        FontExporter.Export(fontextract, stream);
    }

    public static FontExtractOption CreateOption(string export_chars) => new()
    {
        ExtractChars = [.. export_chars.ToUtf32CharArray()],
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

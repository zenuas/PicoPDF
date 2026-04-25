using Mina.Command;
using Mina.Extension;
using OpenType;
using System.IO;

namespace PicoPDF.TestAll;

public class FontExport : FontRegisterCommand
{
    [CommandOption("font")]
    public string Font { get; init; } = "Meiryo Bold";

    [CommandOption("output"), CommandOption('o')]
    public required string Output { get; init; }

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
        var fontextract = FontExtract.Extract(font, CreateOption(args.Join()));
        using var stream = new FileStream(Output, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
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

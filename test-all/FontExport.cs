using Mina.Command;
using Mina.Extension;
using OpenType;
using System.IO;

namespace PicoPDF.TestAll;

public class FontExport : FontRegisterCommand
{
    [CommandOption("font")]
    public string Font { get; init; } = "Meiryo Bold";

    [CommandOption("type")]
    public FontTypes FontType { get; init; } = FontTypes.TrueType;

    [CommandOption("output"), CommandOption('o')]
    public required string Output { get; init; }

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
        var fontextract = FontExtract.Extract(font, new FontExtractOption { ExtractChars = [.. args.Join().ToUtf32CharArray()] });
        using var stream = new FileStream(Output, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        FontExporter.Export(fontextract, FontType, stream);
    }
}

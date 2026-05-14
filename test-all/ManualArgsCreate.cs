using Mina.Command;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf;

namespace PicoPDF.TestAll;

public class ManualArgsCreate : FontRegisterCommand
{
    [CommandOption("output"), CommandOption('o')]
    public string Output { get; init; } = "a.pdf";

    [CommandOption("font")]
    public string Font { get; init; } = "Meiryo Bold";

    [CommandOption("point"), CommandOption('p')]
    public float Point { get; init; } = 100.0F;

    [CommandOption("embed")]
    public FontEmbed FontEmbed { get; init; } = FontEmbed.PossibleEmbed;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var doc = new Document() { FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientation.Horizontal);
        var font = doc.AddFont("fo", fontreg.LoadComplete(Font), FontEmbed);
        foreach (var arg in args)
        {
            var page = doc.NewPage(width, height);
            _ = page.Contents.DrawText(arg, 100, 100, Point, [font]);
        }

        doc.Save(Output, new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

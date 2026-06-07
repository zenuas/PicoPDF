using Mina.Command;
using Pdf.Documents;
using PicoPDF.Loader.Sections;

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
    public FontEmbeds FontEmbed { get; init; } = FontEmbeds.PossibleEmbed;

    [CommandOption("top")]
    public int Top { get; init; } = 100;

    [CommandOption("left")]
    public int Left { get; init; } = 100;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document = new Document() { FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var font = document.AddFont("fo", fontreg.LoadComplete(Font), FontEmbed);
        foreach (var arg in args)
        {
            var page = document.NewPage(width, height);
            page.Contents.Operations.Add(Contents.CreateDrawText(page.Document, arg, Left, Top, Point, [font], width - Left, style: TextStyles.MultiLine));
        }

        document.Save(Output, new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

using Mina.Command;
using Pdf.Documents;
using Pdf.Font;
using Pdf.Operation;
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

        var document = PdfFactory.Create(new() { CreateFontRegister = () => fontreg });
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var font = Type0Font.Create("fo", fontreg.LoadComplete(Font), FontEmbed);
        document.Fonts.Add(font);
        foreach (var arg in args)
        {
            var page = document.NewPage(width, height);
            page.Contents.Operations.Add(DrawString.Create(arg, Left, Top, Point, [font], page.Document, width - Left, style: TextStyles.MultiLine));
        }

        document.Save(Output, new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

using Pdf.Documents;
using Pdf.Font;
using PicoPDF.Loader.Sections;
using System.Text;

namespace PicoPDF.TestAll;

public class ManualCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document = new Document() { FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var page = document.NewPage(width, height);

        var cid = document.AddFont("CID", "HeiseiMin", CMaps.UniJIS_UCS2_H, Encoding.BigEndianUnicode);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("CID Font! あア亜", 100, 100, 12, cid));
        page.Contents.Operations.AddRange(Contents.CreateDrawTextStyleOperations(TextStyles.Underline, 100, 100, 100, 50, 12));

        var stdtype1 = document.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("Standard Type1 Font!", 120, 100, 12, stdtype1));

        var type1 = document.AddFont("TYPE1", "Helvetica", Type1Encodings.WinAnsiEncoding);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("Type1 Font!", 140, 100, 12, type1));

        var post = document.AddFont("POST1", fontreg.LoadComplete("test-case/NotoSansCJK-Regular.ttc,0"));
        page.Contents.Operations.Add(Contents.CreateDrawText(page.Document, "PostScript Font! あア亜 𠮷野家", 100, 160, 12, [post]));

        var ttf = document.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page.Contents.Operations.Add(Contents.CreateDrawText(page.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf]));

        document.Save("test-case/manual-create.pdf", new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

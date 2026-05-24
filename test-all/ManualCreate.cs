using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Font;
using System.Text;

namespace PicoPDF.TestAll;

public class ManualCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var doc = new Document() { FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientation.Horizontal);
        var page = doc.NewPage(width, height);

        var cid = doc.AddFont("CID", "HeiseiMin", CMap.UniJIS_UCS2_H, Encoding.BigEndianUnicode);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("CID Font! あア亜", 100, 100, 12, cid));
        page.Contents.Operations.AddRange(Contents.CreateDrawTextStyleOperations(TextStyles.Underline, 100, 100, 100, 50, 12));

        var stdtype1 = doc.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("Standard Type1 Font!", 120, 100, 12, stdtype1));

        var type1 = doc.AddFont("TYPE1", "Helvetica", Type1Encoding.WinAnsiEncoding);
        page.Contents.Operations.Add(Contents.CreateDrawTextOnBaselineOperation("Type1 Font!", 140, 100, 12, type1));

        var post = doc.AddFont("POST1", fontreg.LoadComplete("test-case/NotoSansCJK-Regular.ttc,0"));
        page.Contents.Operations.Add(Contents.CreateDrawText(page.Document, "PostScript Font! あア亜 𠮷野家", 100, 160, 12, [post]));

        var ttf = doc.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page.Contents.Operations.Add(Contents.CreateDrawText(page.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf]));

        doc.Save("test-case/manual-create.pdf", new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

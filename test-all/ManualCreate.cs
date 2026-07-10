using Pdf.Documents;
using Pdf.Font;
using Pdf.Operation;
using PicoPDF.Loader.Sections;
using System.Text;

namespace PicoPDF.TestAll;

public class ManualCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document = PdfFactory.Create(new() { CreateFontRegister = () => fontreg });
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var page = document.NewPage(width, height);

        var cid = CIDFont.Create("CID", "HeiseiMin", CMaps.UniJIS_UCS2_H, Encoding.BigEndianUnicode);
        document.Fonts.Add(cid);
        page.Contents.Operations.Add(DrawString.Create("CID Font! あア亜", 100, 100, 12, cid));
        page.Contents.Operations.AddRange(DrawOperations.CreateTextStyle(TextStyles.Underline, 100, 100, 100, 50, 12));

        var stdtype1 = StandardType1Font.Create("STDTYPE1", StandardType1Fonts.HelveticaBold);
        document.Fonts.Add(stdtype1);
        page.Contents.Operations.Add(DrawString.Create("Standard Type1 Font!", 120, 100, 12, stdtype1));

        var type1 = Type1Font.Create("TYPE1", "Helvetica", Type1Encodings.WinAnsiEncoding);
        document.Fonts.Add(type1);
        page.Contents.Operations.Add(DrawString.Create("Type1 Font!", 140, 100, 12, type1));

        var post = Type0Font.Create("POST1", fontreg.LoadFont("test-case/NotoSansCJK-Regular.ttc,0"));
        document.Fonts.Add(post);
        page.Contents.Operations.Add(DrawString.Create("PostScript Font! あア亜 𠮷野家", 100, 160, 12, [post], page.Document));

        var ttf = Type0Font.Create("true1", fontreg.LoadFont("Meiryo Bold"));
        document.Fonts.Add(ttf);
        page.Contents.Operations.Add(DrawString.Create("TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf], page.Document));

        document.Save("test-case/manual-create.pdf", new() { ContentsStreamDeflate = false, Debug = true, OutputCrossReferenceTable = false });
    }
}

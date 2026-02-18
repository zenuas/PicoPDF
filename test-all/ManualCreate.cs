using PicoPDF.Binder.Data;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System.Linq;
using System.Text;

namespace PicoPDF.TestAll;

public static class ManualCreate
{
    public static void Run(IFontRegister fontreg, PdfExportOption opt)
    {
        var doc = new Document() { FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientation.Horizontal);
        var page = doc.NewPage(width, height);

        var cid = doc.AddFont("CID", "HeiseiMin", CMap.UniJIS_UCS2_H, Encoding.BigEndianUnicode);
        page.Contents.DrawText("CID Font! あア亜", 100, 100, 12, cid);

        var stdtype1 = doc.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.DrawText("Standard Type1 Font!", 100, 120, 12, stdtype1);

        var type1 = doc.AddFont("TYPE1", "Helvetica", Type1Encoding.WinAnsiEncoding);
        page.Contents.DrawText("Type1 Font!", 100, 140, 12, type1);

        var post = doc.AddFont("POST1", fontreg.LoadRequiredTables("test-case/NotoSansCJK-Regular.ttc,0"));
        page.Contents.DrawText("PostScript Font! あア亜 𠮷野家", 100, 160, 12, post);

        var ttf = doc.AddFont("true1", fontreg.LoadRequiredTables("Meiryo Bold"));
        page.Contents.DrawText("TrueType Font! あア亜 𠮷野家", 100, 180, 12, ttf);

        var emoji = doc.AddFont("emoji1", fontreg.LoadRequiredTables("Segoe UI Emoji"));
        var textfonts = PdfUtility.GetTextFont("aijpqあいうえお🍣", [ttf, emoji]).ToArray();
        page.Contents.DrawTextFont(textfonts, 100, 200, 12);

        _ = page.Contents.DrawMultilineText("途中で\n改行コードの\r\n入った\rテキストのテスト", 220, 100, 12, [ttf, emoji]);

        doc.Save("test-case/manual-create.pdf", opt);
    }
}

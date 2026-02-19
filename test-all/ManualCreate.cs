using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
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
        page.Contents.DrawTextOnBaseline("CID Font! あア亜", 100, 100, 12, cid);
        page.Contents.DrawTextStyle(TextStyle.Underline, 100, 100, 100, 50, 12);

        var stdtype1 = doc.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.DrawTextOnBaseline("Standard Type1 Font!", 120, 100, 12, stdtype1);

        var type1 = doc.AddFont("TYPE1", "Helvetica", Type1Encoding.WinAnsiEncoding);
        page.Contents.DrawTextOnBaseline("Type1 Font!", 140, 100, 12, type1);

        var post = doc.AddFont("POST1", fontreg.LoadRequiredTables("test-case/NotoSansCJK-Regular.ttc,0"));
        _ = page.Contents.DrawText("PostScript Font! あア亜 𠮷野家", 160, 100, 12, [post]);

        var ttf = doc.AddFont("true1", fontreg.LoadRequiredTables("Meiryo Bold"));
        _ = page.Contents.DrawText("TrueType Font! あア亜 𠮷野家", 180, 100, 12, [ttf]);

        var emoji = doc.AddFont("emoji1", fontreg.LoadRequiredTables("Segoe UI Emoji"));
        _ = page.Contents.DrawText("aijpqあいうえお🍣", 200, 100, 12, [ttf, emoji]);

        _ = page.Contents.DrawText("途中で\n改行コードの\r\n入った\rテキストのテスト", 220, 100, 12, [ttf, emoji], style: TextStyle.Border);

        _ = page.Contents.DrawText("途中で\n改行コードの\r\n入った\rテキストのテスト", 220, 300, 12, [ttf, emoji], 65, 60, style: TextStyle.Border | TextStyle.Clipping);

        doc.Save("test-case/manual-create.pdf", opt);
    }
}

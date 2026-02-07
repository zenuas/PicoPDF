using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System.Text;

namespace PicoPDF.TestAll;

public class ManualCreate
{
    public static void Run(IFontRegister fontreg, PdfExportOption opt)
    {
        var doc = new Document() { FontRegister = fontreg };
        var (width, height) = SectionBinder.GetPageSize(PageSize.A4, Orientation.Horizontal);
        var page = doc.NewPage(width, height);

        var cid = doc.AddFont("CID", "HeiseiMin", CMap.UniJIS_UCS2_H, Encoding.BigEndianUnicode);
        page.Contents.DrawString("CID Font! あア亜", 100, 100, 12, cid);

        var stdtype1 = doc.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.DrawString("Standard Type1 Font!", 100, 120, 12, stdtype1);

        var type1 = doc.AddFont("TYPE1", "Helvetica", Type1Encoding.WinAnsiEncoding);
        page.Contents.DrawString("Type1 Font!", 100, 140, 12, type1);

        doc.Save("test-case/manual-create.pdf", opt);
    }
}

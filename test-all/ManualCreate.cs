using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf;
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
        page.Contents.DrawTextOnBaseline("CID Font! あア亜", 100, 100, 12, cid);
        page.Contents.DrawTextStyle(TextStyle.Underline, 100, 100, 100, 50, 12);

        var stdtype1 = doc.AddFont("STDTYPE1", StandardType1Fonts.HelveticaBold);
        page.Contents.DrawTextOnBaseline("Standard Type1 Font!", 120, 100, 12, stdtype1);

        var type1 = doc.AddFont("TYPE1", "Helvetica", Type1Encoding.WinAnsiEncoding);
        page.Contents.DrawTextOnBaseline("Type1 Font!", 140, 100, 12, type1);

        var post = doc.AddFont("POST1", fontreg.LoadComplete("test-case/NotoSansCJK-Regular.ttc,0"));
        _ = page.Contents.DrawText("PostScript Font! あア亜 𠮷野家", 160, 100, 12, [post]);

        var ttf = doc.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        _ = page.Contents.DrawText("TrueType Font! あアģ亜 𠮷野家", 180, 100, 12, [ttf]);

        var emoji = doc.AddFont("emoji1", fontreg.LoadComplete("Segoe UI Emoji"));
        _ = page.Contents.DrawText("aijpqあいうえお👤←Segoe UI Emoji", 200, 100, 12, [ttf, emoji]);

        var emojistroke = doc.AddFont("emoji1", fontreg.LoadComplete("Segoe UI Emoji"), FontEmbed.Stroke);
        _ = page.Contents.DrawText("aijpqあいうえお👤←Segoe UI Emoji(Stroke)", 220, 100, 12, [ttf, emojistroke]);

        var notocolemoji = doc.AddFont("emoji2", fontreg.LoadComplete("test-case/NotoColorEmoji-Regular.ttf"), FontEmbed.Stroke);
        _ = page.Contents.DrawText("aijpqあいうえお👤←NotoColorEmoji-Regular(Stroke)", 240, 100, 12, [ttf, notocolemoji]);

        _ = page.Contents.DrawText("途中で\n改行コードの\r\n入った\rテキストのテスト", 260, 100, 12, [ttf, emoji], style: TextStyle.Border);

        _ = page.Contents.DrawText("途中で\n改行コードの\r\n入った\rテキストのテスト", 260, 300, 12, [ttf, emoji], 65, 60, style: TextStyle.Border | TextStyle.Clipping);

        _ = page.Contents.DrawText("TrueTypeでStroke化🍣", 320, 100, 12, [ttf, emoji], style: TextStyle.Stroke | TextStyle.Border);
        _ = page.Contents.DrawText("PostScriptでStroke化🍣", 340, 100, 12, [post, emoji], style: TextStyle.Stroke | TextStyle.Border);

        _ = page.Contents.DrawText("TrueTypeでフォントフォールバックできない⇒🍣", 360, 100, 12, [ttf]);
        _ = page.Contents.DrawText("PostScriptでフォントフォールバックできない⇒🍣", 380, 100, 12, [post]);

        doc.Save("test-case/manual-create.pdf", new() { ContentsStreamDeflate = false, Debug = true });
    }
}

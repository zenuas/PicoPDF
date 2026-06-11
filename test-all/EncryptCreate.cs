using Pdf.Documents;
using Pdf.Documents.Security;
using PicoPDF.Loader.Sections;
using System;

namespace PicoPDF.TestAll;

public class EncryptCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document1 = new Document() { Version = 17, FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var page1 = document1.NewPage(width, height);
        var ttf1 = document1.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page1.Contents.Operations.Add(Contents.CreateDrawText(page1.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf1]));
        document1.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document1.AddEncrypt(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default);
        document1.Save("test-case/encrypt-none-create.pdf", new() { ContentsStreamDeflate = false });

        var document2 = new Document() { Version = 17, FontRegister = fontreg };
        var page2 = document2.NewPage(width, height);
        var ttf2 = document2.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page2.Contents.Operations.Add(Contents.CreateDrawText(page2.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf2]));
        document2.AddEncrypt(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default);
        document2.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document2.Save("test-case/encrypt-aesv2-create.pdf");

        var document3 = new Document() { Version = 20, FontRegister = fontreg };
        var page3 = document3.NewPage(width, height);
        var ttf3 = document3.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page3.Contents.Operations.Add(Contents.CreateDrawText(page3.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf3]));
        document3.AddEncrypt(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default);
        document3.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document3.Save("test-case/encrypt-aesv3-create.pdf");

        var document4 = new Document() { Version = 17, FontRegister = fontreg };
        var page4 = document4.NewPage(width, height);
        var ttf4 = document4.AddFont("true1", fontreg.LoadComplete("Meiryo Bold"));
        page4.Contents.Operations.Add(Contents.CreateDrawText(page4.Document, "TrueType Font! あア亜 𠮷野家", 100, 180, 12, [ttf4]));
        document4.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document4.Save("test-case/encrypt-noencrypt-create.pdf", new() { ContentsStreamDeflate = false });
    }
}

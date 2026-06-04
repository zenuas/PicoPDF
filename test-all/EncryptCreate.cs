using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Documents.Security;
using System;

namespace PicoPDF.TestAll;

public class EncryptCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document1 = new Document() { Version = 17, FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        _ = document1.NewPage(width, height);
        document1.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document1.AddEncrypt(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default);
        document1.Save("test-case/encrypt-none-create.pdf");

        var document2 = new Document() { Version = 17, FontRegister = fontreg };
        _ = document2.NewPage(width, height);
        document2.AddEncrypt(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default);
        document2.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document2.Save("test-case/encrypt-aesv2-create.pdf");

        var document3 = new Document() { Version = 20, FontRegister = fontreg };
        _ = document3.NewPage(width, height);
        document3.AddEncrypt(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default);
        document3.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document3.Save("test-case/encrypt-aesv3-create.pdf");

        var document4 = new Document() { Version = 17, FontRegister = fontreg };
        _ = document4.NewPage(width, height);
        document4.AddInfo("Titleあいう", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document4.Save("test-case/encrypt-noencrypt-create.pdf");
    }
}

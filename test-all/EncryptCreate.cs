using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Documents.Security;

namespace PicoPDF.TestAll;

public class EncryptCreate : FontRegisterCommand
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document1 = new Document() { Version = 17, FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        _ = document1.NewPage(width, height);
        document1.AddEncrypt(CFM.None, "abc123", UserAccessPermissions.Default);
        document1.Save("test-case/encrypt-none-create.pdf");

        var document2 = new Document() { Version = 17, FontRegister = fontreg };
        _ = document2.NewPage(width, height);
        document2.AddEncrypt(CFM.AESV2, "abc123", UserAccessPermissions.Default);
        document2.Save("test-case/encrypt-aesv2-create.pdf");

        var document3 = new Document() { Version = 20, FontRegister = fontreg };
        _ = document3.NewPage(width, height);
        document3.AddEncrypt(CFM.AESV3, "abc123", UserAccessPermissions.Default);
        document3.Save("test-case/encrypt-aesv3-create.pdf");
    }
}

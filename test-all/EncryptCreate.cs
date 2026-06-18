using Mina.Command;
using Pdf.Documents;
using Pdf.Documents.Security;
using Pdf.Drawing;
using Pdf.Operation;
using PicoPDF.Loader.Sections;
using System;

namespace PicoPDF.TestAll;

public class EncryptCreate : FontRegisterCommand
{

    [CommandOption("work-directory")]
    public string WorkDirectory { get; init; } = "test-case";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var document1 = new Document() { Version = 17, FontRegister = fontreg };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var page1 = document1.NewPage(width, height);
        page1.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document1.AddInfo("Title", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document1.AddEncrypt(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default);
        document1.Save($"{WorkDirectory}/encrypt-none17-create.pdf", new() { ContentsStreamDeflate = false });

        var document2 = new Document() { Version = 20, FontRegister = fontreg };
        var page2 = document2.NewPage(width, height);
        page2.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document2.AddInfo("Title", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document2.AddEncrypt(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default);
        document2.Save($"{WorkDirectory}/encrypt-none20-create.pdf", new() { ContentsStreamDeflate = false });

        var document3 = new Document() { Version = 17, FontRegister = fontreg };
        _ = document3.NewPage(width, height);
        document3.AddEncrypt(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default);
        document3.AddInfo("Title", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document3.Save($"{WorkDirectory}/encrypt-aesv2-create.pdf");

        var document4 = new Document() { Version = 20, FontRegister = fontreg };
        _ = document4.NewPage(width, height);
        document4.AddEncrypt(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default);
        document4.AddInfo("Title", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document4.Save($"{WorkDirectory}/encrypt-aesv3-create.pdf");

        var document5 = new Document() { Version = 17, FontRegister = fontreg };
        _ = document5.NewPage(width, height);
        document5.AddInfo("Title", creation_date: new DateTime(2000, 1, 2, 3, 4, 5));
        document5.Save($"{WorkDirectory}/encrypt-noencrypt-create.pdf", new() { ContentsStreamDeflate = false });
    }
}

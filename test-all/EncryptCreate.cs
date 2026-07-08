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

        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        var document1 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption4.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID()),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        var page1 = document1.NewPage(width, height);
        page1.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document1.Save($"{WorkDirectory}/encrypt-none17-create.pdf", new() { ContentsStreamDeflate = false });

        var document2 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption6.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        var page2 = document2.NewPage(width, height);
        page2.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document2.Save($"{WorkDirectory}/encrypt-none20-create.pdf", new() { ContentsStreamDeflate = false });

        var document3 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID()),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        _ = document3.NewPage(width, height);
        document3.Save($"{WorkDirectory}/encrypt-aesv2-create.pdf", new() { ContentsStreamDeflate = false });

        var document4 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        _ = document4.NewPage(width, height);
        document4.Save($"{WorkDirectory}/encrypt-aesv3-create.pdf", new() { ContentsStreamDeflate = false });

        var document5 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID(), false),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        _ = document5.NewPage(width, height);
        document5.Save($"{WorkDirectory}/encrypt-aesv2-create-nometaencrypted.pdf", new() { ContentsStreamDeflate = false });

        var document6 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateStandardEncryption = () => StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default, false),
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        _ = document6.NewPage(width, height);
        document6.Save($"{WorkDirectory}/encrypt-aesv3-create-nometaencrypted.pdf", new() { ContentsStreamDeflate = false });

        var document7 = PdfFactory.Create(new()
        {
            CreateFontRegister = () => fontreg,
            CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
        });
        _ = document7.NewPage(width, height);
        document7.Save($"{WorkDirectory}/encrypt-noencrypt-create.pdf", new() { ContentsStreamDeflate = false });
    }
}

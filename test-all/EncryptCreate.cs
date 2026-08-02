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
        (string, IStandardEncryption?)[] encrypt_settings = [
                ("encrypt-none17-create.pdf", StandardEncryption4.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID())),
                ("encrypt-none20-create.pdf", StandardEncryption6.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default)),
                ("encrypt-aesv2-create.pdf", StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID())),
                ("encrypt-aesv3-create.pdf", StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default)),
                ("encrypt-aesv2-create-nometaencrypted.pdf", StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID(), false)),
                ("encrypt-aesv3-create-nometaencrypted.pdf", StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default, false)),
                ("encrypt-noencrypt-create.pdf", null),
            ];

        var fontreg = CreateFontRegister();
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        foreach (var (filename, encryption) in encrypt_settings)
        {
            var document = PdfFactory.Create(new()
            {
                CreateFontRegister = () => fontreg,
                CreateStandardEncryption = () => encryption,
                CreateMetadata = () => new XmpMetadata() { CreateDate = DateTime.Now, Keywords = "keyword" },
            });
            var page = document.NewPage(width, height);
            page.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
            document.Save($"{WorkDirectory}/{filename}", new() { ContentsStreamDeflate = false });
        }
    }
}

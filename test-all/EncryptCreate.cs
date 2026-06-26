using Mina.Command;
using Mina.Extension;
using Pdf.Documents;
using Pdf.Documents.Security;
using Pdf.Drawing;
using Pdf.Operation;
using PicoPDF.Loader.Sections;

namespace PicoPDF.TestAll;

public class EncryptCreate : FontRegisterCommand
{
    [CommandOption("work-directory")]
    public string WorkDirectory { get; init; } = "test-case";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        var encrypt1 = StandardEncryption4.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID());
        var document1 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Encrypt = encrypt1,
            StreamHandler = encrypt1.StreamHandler,
            StringHandler = encrypt1.StringHandler,
            EmbeddedFileStreamsHandler = encrypt1.EmbeddedFileStreamsHandler,
            DocumentID = (encrypt1.DocumentID, encrypt1.DocumentID),
        };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
        document1.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document1.Catalog.RelatedObjects.Add));
        var page1 = document1.NewPage(width, height);
        page1.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document1.Save($"{WorkDirectory}/encrypt-none17-create.pdf", new() { ContentsStreamDeflate = false });

        var encrypt2 = StandardEncryption6.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default);
        var document2 = new Document()
        {
            Version = 20,
            FontRegister = fontreg,
            Encrypt = encrypt2,
            StreamHandler = encrypt2.StreamHandler,
            StringHandler = encrypt2.StringHandler,
            EmbeddedFileStreamsHandler = encrypt2.EmbeddedFileStreamsHandler,
        };
        document2.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document2.Catalog.RelatedObjects.Add));
        var page2 = document2.NewPage(width, height);
        page2.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document2.Save($"{WorkDirectory}/encrypt-none20-create.pdf", new() { ContentsStreamDeflate = false });

        var encrypt3 = StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID());
        var document3 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Encrypt = encrypt3,
            StreamHandler = encrypt3.StreamHandler,
            StringHandler = encrypt3.StringHandler,
            EmbeddedFileStreamsHandler = encrypt3.EmbeddedFileStreamsHandler,
            DocumentID = (encrypt3.DocumentID, encrypt3.DocumentID),
        };
        document3.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document3.Catalog.RelatedObjects.Add));
        _ = document3.NewPage(width, height);
        document3.Save($"{WorkDirectory}/encrypt-aesv2-create.pdf", new() { ContentsStreamDeflate = false });

        var encrypt4 = StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default);
        var document4 = new Document()
        {
            Version = 20,
            FontRegister = fontreg,
            Encrypt = encrypt4,
            StreamHandler = encrypt4.StreamHandler,
            StringHandler = encrypt4.StringHandler,
            EmbeddedFileStreamsHandler = encrypt4.EmbeddedFileStreamsHandler,
        };
        document4.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document4.Catalog.RelatedObjects.Add));
        _ = document4.NewPage(width, height);
        document4.Save($"{WorkDirectory}/encrypt-aesv3-create.pdf", new() { ContentsStreamDeflate = false });

        var encrypt5 = StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, Document.GenerateID(), false);
        var document5 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Encrypt = encrypt5,
            StreamHandler = encrypt5.StreamHandler,
            StringHandler = encrypt5.StringHandler,
            EmbeddedFileStreamsHandler = encrypt5.EmbeddedFileStreamsHandler,
            DocumentID = (encrypt5.DocumentID, encrypt5.DocumentID),
        };
        document5.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document5.Catalog.RelatedObjects.Add));
        _ = document5.NewPage(width, height);
        document5.Save($"{WorkDirectory}/encrypt-aesv2-create-nometaencrypted.pdf", new() { ContentsStreamDeflate = false });

        var encrypt6 = StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default, false);
        var document6 = new Document()
        {
            Version = 20,
            FontRegister = fontreg,
            Encrypt = encrypt6,
            StreamHandler = encrypt6.StreamHandler,
            StringHandler = encrypt6.StringHandler,
            EmbeddedFileStreamsHandler = encrypt6.EmbeddedFileStreamsHandler,
        };
        document6.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document6.Catalog.RelatedObjects.Add));
        _ = document6.NewPage(width, height);
        document6.Save($"{WorkDirectory}/encrypt-aesv3-create-nometaencrypted.pdf", new() { ContentsStreamDeflate = false });

        var document7 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
        };
        document7.Catalog.Elements.Add("Metadata", new XmpMetadata().Return(document7.Catalog.RelatedObjects.Add));
        _ = document7.NewPage(width, height);
        document7.Save($"{WorkDirectory}/encrypt-noencrypt-create.pdf", new() { ContentsStreamDeflate = false });
    }
}

using Mina.Command;
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

        var id1 = Document.GenerateID();
        var encrypt1 = StandardEncryption4.Create(CFM.None, "xyz987", "abc123", UserAccessPermissions.Default, id1);
        var document1 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Encrypt = encrypt1,
            StreamHandler = encrypt1.StreamHandler,
            StringHandler = encrypt1.StringHandler,
            EmbeddedFileStreamsHandler = encrypt1.EmbeddedFileStreamsHandler,
            DocumentID = (id1, id1),
            Info = new() { Title = "Title", CreationDate = new(2000, 1, 2, 3, 4, 5) },
        };
        var (width, height) = PageSize.GetPageSize(PageSizes.A4, Orientations.Horizontal);
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
            Info = new() { Title = "Title", CreationDate = new(2000, 1, 2, 3, 4, 5) },
        };
        var page2 = document2.NewPage(width, height);
        page2.Contents.Operations.Add(new DrawLine { Points = [(new PointValue(50), new PointValue(75)), (new PointValue(100), new PointValue(125))], LineWidth = new PointValue(10) });
        document2.Save($"{WorkDirectory}/encrypt-none20-create.pdf", new() { ContentsStreamDeflate = false });

        var id3 = Document.GenerateID();
        var encrypt3 = StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, id3);
        var document3 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Encrypt = encrypt3,
            StreamHandler = encrypt3.StreamHandler,
            StringHandler = encrypt3.StringHandler,
            EmbeddedFileStreamsHandler = encrypt3.EmbeddedFileStreamsHandler,
            DocumentID = (id3, id3),
            Info = new() { Title = "Title", CreationDate = new(2000, 1, 2, 3, 4, 5) },
        };
        _ = document3.NewPage(width, height);
        document3.Save($"{WorkDirectory}/encrypt-aesv2-create.pdf");

        var encrypt4 = StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default);
        var document4 = new Document()
        {
            Version = 20,
            FontRegister = fontreg,
            Encrypt = encrypt4,
            StreamHandler = encrypt4.StreamHandler,
            StringHandler = encrypt4.StringHandler,
            EmbeddedFileStreamsHandler = encrypt4.EmbeddedFileStreamsHandler,
            Info = new() { Title = "Title", CreationDate = new(2000, 1, 2, 3, 4, 5) },
        };
        _ = document4.NewPage(width, height);
        document4.Save($"{WorkDirectory}/encrypt-aesv3-create.pdf");

        var document5 = new Document()
        {
            Version = 17,
            FontRegister = fontreg,
            Info = new() { Title = "Title", CreationDate = new(2000, 1, 2, 3, 4, 5) },
        };
        _ = document5.NewPage(width, height);
        document5.Save($"{WorkDirectory}/encrypt-noencrypt-create.pdf", new() { ContentsStreamDeflate = false });
    }
}

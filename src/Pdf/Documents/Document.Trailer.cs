using PicoPDF.Pdf.Documents.Security;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Extension;
using System;
using System.Text;

namespace PicoPDF.Pdf.Documents;

public partial class Document
{
    public PdfObject? Info { get; set; }
    public PdfObject? Encrypt { get; set; }
    public ISecurityHandler? StreamHandler { get; set; } = null;
    public ISecurityHandler? StringHandler { get; set; } = null;
    public ISecurityHandler? EmbeddedFileStreamsHandler { get; set; } = null;
    public (byte[] CreateID, byte[] UpdateID)? DocumentID { get; set; }


    public static byte[] GenerateID() => Guid.NewGuid().ToByteArray();

    public (byte[] CreateID, byte[] UpdateID) GetDocumentID()
    {
        if (DocumentID is { } d) return d;
        var create_id = GenerateID();
        DocumentID = (create_id, create_id);
        return ((byte[] CreateID, byte[] UpdateID))DocumentID;
    }

    public void AddInfo(
            string? title = null,
            string? author = null,
            string? subject = null,
            string? keywords = null,
            string? creator = null,
            string? producer = null,
            DateTime? creation_date = null,
            DateTime? mod_date = null,
            string? trapped = null
        )
    {
        if (Info is { }) _ = PdfObjects.Remove(Info);
        PdfObjects.Add(Info = new());
        if (title is { }) Info.Elements.Add("Title", new ElementString { Value = title });
        if (author is { }) Info.Elements.Add("Author", new ElementString { Value = author });
        if (subject is { }) Info.Elements.Add("Subject", new ElementString { Value = subject });
        if (keywords is { }) Info.Elements.Add("Keywords", new ElementString { Value = keywords });
        if (creator is { }) Info.Elements.Add("Creator", new ElementString { Value = creator });
        if (producer is { }) Info.Elements.Add("Producer", new ElementString { Value = producer });
        if (creation_date is { }) Info.Elements.Add("CreationDate", creation_date);
        if (mod_date is { }) Info.Elements.Add("ModDate", mod_date);
        if (trapped is { }) Info.Elements.Add("Trapped", trapped);
    }

    public void AddEncrypt(
            CFM cfm,
            string user_password,
            string owner_password,
            UserAccessPermissions permissions
        )
    {
        StreamHandler = StringHandler = EmbeddedFileStreamsHandler = null;
        if (Encrypt is { }) _ = PdfObjects.Remove(Encrypt);

        PdfObject encrypt;
        switch (cfm)
        {
            case CFM.None when Version >= 20:
                (encrypt, _) = CreateStandardEncryptionNone(5, 6, permissions, GetDocumentID().CreateID);
                StreamHandler = StringHandler = EmbeddedFileStreamsHandler = new IdentityHandler();
                break;

            case CFM.None:
                (encrypt, _) = CreateStandardEncryptionNone(4, 4, permissions, GetDocumentID().CreateID);
                StreamHandler = StringHandler = EmbeddedFileStreamsHandler = new IdentityHandler();
                break;

            case CFM.AESV2:
                (encrypt, var encryption_key) = CreateStandardEncryptionAes128(user_password, owner_password, permissions, GetDocumentID().CreateID);
                StreamHandler = StringHandler = EmbeddedFileStreamsHandler = new Aes128Handler() { Key = encryption_key };
                break;

            case CFM.AESV3:
                (encrypt, _) = CreateStandardEncryptionAes256(permissions);
                //StreamHandler = StringHandler = EmbeddedFileStreamsHandler = new Aes256Handler();
                break;

            default:
                throw new();
        }
        PdfObjects.Add(Encrypt = encrypt);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionNone(int version, int revision, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes([]);
        var owner_password_bytes = Encoding.UTF8.GetBytes([]);
        var o = Encryption.ComputeOwnerPassword_Algorithm3_3(
            user_password_bytes,
            owner_password_bytes,
            16
        );
        var encryption_key = Encryption.ComputeEncryptionKey_Algorithm3_2(
            user_password_bytes,
            o,
            permissions,
            document_id,
            true
        );
        return (new()
        {
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default),
                ["V"] = version,
                ["CF"] = "<< /StdCF << /CFM /None /AuthEvent /DocOpen >> >>",
                ["R"] = revision,
                ["O"] = o.ToHexString(),
                ["U"] = Encryption.ComputeUserPassword_Algorithm3_5(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["EFF"] = "/StdCF",
            }
        }, encryption_key);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionAes128(string user_password, string owner_password, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);
        var o = Encryption.ComputeOwnerPassword_Algorithm3_3(
            user_password_bytes,
            owner_password_bytes,
            16
        );
        var encryption_key = Encryption.ComputeEncryptionKey_Algorithm3_2(
            user_password_bytes,
            o,
            permissions,
            document_id,
            true
        );
        return (new()
        {
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default),
                ["V"] = 4,
                ["CF"] = "<< /StdCF << /CFM /AESV2 /AuthEvent /DocOpen /Length 128 >> >>",
                ["R"] = 4,
                ["O"] = o.ToHexString(),
                ["U"] = Encryption.ComputeUserPassword_Algorithm3_5(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["EFF"] = "/StdCF",
            }
        }, encryption_key);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionAes256(UserAccessPermissions permissions)
    {
        return (new()
        {
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default),
                ["V"] = 5,
                ["CF"] = "<< /StdCF << /CFM /AESV3 /AuthEvent /DocOpen >> >>",
                ["R"] = 6,
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["EFF"] = "/StdCF",
            }
        }, []);
    }
}

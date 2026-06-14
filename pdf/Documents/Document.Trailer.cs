using Pdf.Documents.Security;
using Pdf.Elements;
using Pdf.Extension;
using System;
using System.Text;

namespace Pdf.Documents;

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
                StreamHandler = StringHandler = new IdentityHandler();
                break;

            case CFM.None:
                (encrypt, _) = CreateStandardEncryptionNone(4, 4, permissions, GetDocumentID().CreateID);
                StreamHandler = StringHandler = new IdentityHandler();
                break;

            case CFM.AESV2:
                {
                    (encrypt, var encryption_key) = CreateStandardEncryptionAes128(user_password, owner_password, permissions, GetDocumentID().CreateID);
                    StreamHandler = StringHandler = new Aes128Handler() { Key = encryption_key };
                    break;
                }

            case CFM.AESV3:
                {
                    (encrypt, var encryption_key) = CreateStandardEncryptionAes256(user_password, owner_password, permissions);
                    StreamHandler = StringHandler = new Aes256Handler() { Key = encryption_key };
                    break;
                }

            default:
                throw new();
        }
        PdfObjects.Add(Encrypt = encrypt);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionNone(int version, int revision, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes([]);
        var owner_password_bytes = Encoding.UTF8.GetBytes([]);
        Span<byte> o_key = stackalloc byte[32];
        Aes128Handler.ComputeOwnerPassword_Algorithm3(
            user_password_bytes,
            owner_password_bytes,
            16,
            o_key
        );
        var encryption_key = Aes128Handler.ComputeEncryptionKey_Algorithm2(
            user_password_bytes,
            o_key,
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
                ["O"] = o_key.ToHexString(),
                ["U"] = Aes128Handler.ComputeUserPassword_Algorithm5(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
            }
        }, encryption_key);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionAes128(string user_password, string owner_password, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);
        Span<byte> o_key = stackalloc byte[32];
        Aes128Handler.ComputeOwnerPassword_Algorithm3(
            user_password_bytes,
            owner_password_bytes,
            16,
            o_key
        );
        var encryption_key = Aes128Handler.ComputeEncryptionKey_Algorithm2(
            user_password_bytes,
            o_key,
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
                ["O"] = o_key.ToHexString(),
                ["U"] = Aes128Handler.ComputeUserPassword_Algorithm5(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
            }
        }, encryption_key);
    }

    public static (PdfObject Encrypt, byte[] EncryptionKey) CreateStandardEncryptionAes256(string user_password, string owner_password, UserAccessPermissions permissions)
    {
        // Truncate the UTF-8 representation to 127 bytes if it is longer than 127 bytes.
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);

        var file_encryption_key = Aes256Handler.CreateFileEncryptionKey();
        Span<byte> u_key = stackalloc byte[48];
        Span<byte> ue_key = stackalloc byte[32];
        Aes256Handler.ComputeUserPassword_Algorithm8(user_password_bytes.Length > 127 ? user_password_bytes[..127] : user_password_bytes, file_encryption_key, u_key, ue_key);

        Span<byte> o_key = stackalloc byte[48];
        Span<byte> oe_key = stackalloc byte[32];
        Aes256Handler.ComputeOwnerPassword_Algorithm9(owner_password_bytes.Length > 127 ? owner_password_bytes[..127] : owner_password_bytes, file_encryption_key, u_key, o_key, oe_key);

        Span<byte> perms = stackalloc byte[16];
        Aes256Handler.ComputePerms_Algorithm10(permissions, true, file_encryption_key, perms);

        return (new()
        {
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default_PDF20),
                ["V"] = 5,
                ["CF"] = "<< /StdCF << /CFM /AESV3 /AuthEvent /DocOpen >> >>",
                ["R"] = 6,
                ["O"] = o_key.ToHexString(),
                ["U"] = u_key.ToHexString(),
                ["OE"] = oe_key.ToHexString(),
                ["UE"] = ue_key.ToHexString(),
                ["Perms"] = perms.ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["Length"] = 256, // NOTE (2020) The Length key was corrected to be required and the descriptive text updated.
            }
        }, file_encryption_key);
    }
}

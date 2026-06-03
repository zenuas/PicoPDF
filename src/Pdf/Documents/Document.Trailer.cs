using Mina.Text;
using PicoPDF.Pdf.Documents.Security;
using PicoPDF.Pdf.Extension;
using System;
using System.Text;

namespace PicoPDF.Pdf.Documents;

public partial class Document
{
    public PdfObject? Info { get; set; }
    public PdfObject? Encrypt { get; set; }
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
        if (title is { }) Info.Elements.Add("Title", title.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
        if (author is { }) Info.Elements.Add("Author", author.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
        if (subject is { }) Info.Elements.Add("Subject", subject.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
        if (keywords is { }) Info.Elements.Add("Keywords", keywords.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
        if (creator is { }) Info.Elements.Add("Creator", creator.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
        if (producer is { }) Info.Elements.Add("Producer", producer.ToEscapeString(UTF16WithBOM.UTF16_BEWithBOM));
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
        if (Encrypt is { }) _ = PdfObjects.Remove(Encrypt);
        //if (cfm == CFM.None) return;

        PdfObjects.Add(Encrypt = cfm switch
        {
            CFM.None when Version >= 20 => CreateStandardEncryptionNone(5, 6, permissions),
            CFM.None => CreateStandardEncryptionNone(4, 4, permissions),
            CFM.AESV2 => CreateStandardEncryptionAes128(user_password, owner_password, permissions, GetDocumentID().CreateID),
            CFM.AESV3 => CreateStandardEncryptionAes256(permissions),
            _ => throw new()
        });
    }

    public static PdfObject CreateStandardEncryptionNone(int version, int revision, UserAccessPermissions permissions) => new()
    {
        Elements = {
            ["Filter"] = "/Standard",
            ["P"] = (int)(permissions | UserAccessPermissions.Default),
            ["V"] = version,
            ["CF"] = "<< /StdCF << /CFM /None /AuthEvent /DocOpen >> >>",
            ["R"] = revision,
            ["StmF"] = "/StdCF",
            ["StrF"] = "/StdCF",
            ["EFF"] = "/StdCF"
        }
    };

    public static PdfObject CreateStandardEncryptionAes128(string user_password, string owner_password, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);
        var o = Encryption.ComputeOwnerPassword_Revision3(
            user_password_bytes,
            owner_password_bytes,
            16
        );
        var encryption_key = Encryption.ComputeEncryptionKey_Revision4(
            user_password_bytes,
            o,
            permissions,
            document_id,
            true
        );
        return new()
        {
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default),
                ["V"] = 4,
                ["CF"] = "<< /StdCF << /CFM /AESV2 /AuthEvent /DocOpen /Length 128 >> >>",
                ["Length"] = 128,
                ["R"] = 4,
                ["O"] = o.ToHexString(),
                ["U"] = Encryption.ComputeUserPassword_Revision2(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["EFF"] = "/StdCF"
            }
        };
    }

    public static PdfObject CreateStandardEncryptionAes256(UserAccessPermissions permissions)
    {
        return new()
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
                ["EFF"] = "/StdCF"
            }
        };
    }
}

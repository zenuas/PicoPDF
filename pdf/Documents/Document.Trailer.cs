using Pdf.Documents.Security;
using Pdf.Elements;
using System;

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

        switch (cfm)
        {
            case CFM.None when Version >= 20:
                {
                    (Encrypt, _) = Aes256Handler.Create(cfm, user_password, owner_password, permissions);
                    StreamHandler = StringHandler = new IdentityHandler();
                    break;
                }

            case CFM.None:
                {
                    (Encrypt, _) = Aes128Handler.Create(cfm, user_password, owner_password, permissions, GetDocumentID().CreateID);
                    StreamHandler = StringHandler = new IdentityHandler();
                    break;
                }

            case CFM.AESV2:
                {
                    (Encrypt, var encryption_key) = Aes128Handler.Create(cfm, user_password, owner_password, permissions, GetDocumentID().CreateID);
                    StreamHandler = StringHandler = new Aes128Handler() { Key = encryption_key };
                    break;
                }

            case CFM.AESV3:
                {
                    (Encrypt, var encryption_key) = Aes256Handler.Create(cfm, user_password, owner_password, permissions);
                    StreamHandler = StringHandler = new Aes256Handler() { Key = encryption_key };
                    break;
                }

            default:
                throw new();
        }
        PdfObjects.Add(Encrypt);
    }
}

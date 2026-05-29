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
            string password,
            UserAccessPermissions permissions
        )
    {
        if (Encrypt is { }) _ = PdfObjects.Remove(Encrypt);
        //if (cfm == CFM.None) return;

        var p = (int)(permissions | UserAccessPermissions.Default);
        PdfObjects.Add(Encrypt = new());
        Encrypt.Elements.Add("Filter", "/Standard");
        Encrypt.Elements.Add("P", p);
        switch (cfm)
        {
            case CFM.None when Version >= 20:
                Encrypt.Elements.Add("V", 5);
                Encrypt.Elements.Add("CF", $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen >> >>");
                Encrypt.Elements.Add("R", 6);
                break;

            case CFM.None:
                Encrypt.Elements.Add("V", 4);
                Encrypt.Elements.Add("CF", $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen >> >>");
                Encrypt.Elements.Add("R", 4);
                break;

            case CFM.AESV2:
                Encrypt.Elements.Add("V", 4);
                Encrypt.Elements.Add("CF", $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen /Length 128 >> >>");
                Encrypt.Elements.Add("Length", 128);
                Encrypt.Elements.Add("R", 4);
                Encrypt.Elements.Add("O", Encryption.CreatePassword_Revision4(Encoding.UTF8.GetBytes(password), [], p, [], 128 / 8).ToHexString());
                break;

            case CFM.AESV3:
                Encrypt.Elements.Add("V", 5);
                Encrypt.Elements.Add("CF", $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen >> >>");
                Encrypt.Elements.Add("R", 6);
                break;

            default:
                throw new();
        }
        Encrypt.Elements.Add("StmF", "/StdCF");
        Encrypt.Elements.Add("StrF", "/StdCF");
        Encrypt.Elements.Add("EFF", "/StdCF");
    }
}

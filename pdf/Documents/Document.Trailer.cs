using Pdf.Documents.Security;
using Pdf.Elements;
using System;

namespace Pdf.Documents;

public partial class Document
{
    public PdfObject? Info { get; set; }
    public PdfObject? Encrypt { get; init { if (value is { }) PdfObjects.Add(field = value); } } = null;
    public ISecurityHandler? StreamHandler { get; init; } = null;
    public ISecurityHandler? StringHandler { get; init; } = null;
    public ISecurityHandler? EmbeddedFileStreamsHandler { get; init; } = null;
    public (byte[] CreateID, byte[] UpdateID)? DocumentID { get; init; }


    public static byte[] GenerateID() => Guid.NewGuid().ToByteArray();

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
}

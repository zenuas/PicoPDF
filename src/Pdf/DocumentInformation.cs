using System;
using System.Linq;

namespace PicoPDF.Pdf;

public class DocumentInformation : PdfObject
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string? Subject { get; init; }
    public string? Keywords { get; init; }
    public string? Creator { get; init; }
    public string? Producer { get; init; }
    public DateTime? CreationDate { get; init; }
    public DateTime? ModDate { get; init; }
    public string? Trapped { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        if (Title is { }) _ = Elements.TryAdd("Title", PdfUtility.ToStringEscapeBytes(Title).ToArray());
        if (Author is { }) _ = Elements.TryAdd("Author", PdfUtility.ToStringEscapeBytes(Author).ToArray());
        if (Subject is { }) _ = Elements.TryAdd("Subject", PdfUtility.ToStringEscapeBytes(Subject).ToArray());
        if (Keywords is { }) _ = Elements.TryAdd("Keywords", PdfUtility.ToStringEscapeBytes(Keywords).ToArray());
        if (Creator is { }) _ = Elements.TryAdd("Creator", PdfUtility.ToStringEscapeBytes(Creator).ToArray());
        if (Producer is { }) _ = Elements.TryAdd("Producer", PdfUtility.ToStringEscapeBytes(Producer).ToArray());
        if (CreationDate is { }) _ = Elements.TryAdd("CreationDate", CreationDate);
        if (ModDate is { }) _ = Elements.TryAdd("ModDate", ModDate);
        if (Trapped is { }) _ = Elements.TryAdd("Trapped", Trapped);
    }
}

using Pdf.Elements;
using System;

namespace Pdf.Documents;

public class TrailerInfo : PdfObject, IMetadata
{
    public string? Title { get; init; } = null;
    public string? Author { get; init; } = null;
    public string? Subject { get; init; } = null;
    public string? Keywords { get; init; } = null;
    public string? Creator { get; init; } = null;
    public string? Producer { get; init; } = null;
    public DateTime? CreationDate { get; init; } = null;
    public DateTime? ModDate { get; init; } = null;
    public Trappeds? Trapped { get; init; } = null;

    public override void BeforeExport(PdfExportOption option)
    {
        if (Title is { }) Elements.Add("Title", new ElementString { Value = Title });
        if (Author is { }) Elements.Add("Author", new ElementString { Value = Author });
        if (Subject is { }) Elements.Add("Subject", new ElementString { Value = Subject });
        if (Keywords is { }) Elements.Add("Keywords", new ElementString { Value = Keywords });
        if (Creator is { }) Elements.Add("Creator", new ElementString { Value = Creator });
        if (Producer is { }) Elements.Add("Producer", new ElementString { Value = Producer });
        if (CreationDate is { }) Elements.Add("CreationDate", CreationDate);
        if (ModDate is { }) Elements.Add("ModDate", ModDate);
        if (Trapped is { }) Elements.Add("Trapped", Trapped.ToString()!);
    }
}

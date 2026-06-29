using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Pdf.Documents;

public class XmpMetadata : PdfObject, IMetadata
{
    // XMP namespace
    public DateTime? CreateDate { get; init; } = null;
    public string? CreatorTool { get; init; } = null;
    public string? Identifier { get; init; } = null;
    public string? Label { get; init; } = null;
    public DateTime? MetadataDate { get; init; } = null;
    public DateTime? ModifyDate { get; init; } = null;
    public int? Rating { get; init; } = null;

    // Adobe PDF namespace
    public string? Keywords { get; init; } = null;
    public int? PDFVersion { get; init; } = null;
    public string? Producer { get; init; } = null;
    public bool? Trapped { get; init; } = null;

    public XmpMetadata()
    {
        _ = Elements.TryAdd("Type", "/Metadata");
        _ = Elements.TryAdd("Subtype", "/XML");
    }

    public override void BeforeExport(PdfExportOption option)
    {
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        writer.Write("<?xpacket begin=\"\U0000FEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n"u8);
        writer.Write("<xmpmeta xmlns=\"adobe:ns:meta/\">\n"u8);
        writer.Write("  <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n"u8);

        // XMP namespace
        if (CreateDate is { } ||
            CreatorTool is { } ||
            Identifier is { } ||
            Label is { } ||
            MetadataDate is { } ||
            ModifyDate is { } ||
            Rating is { })
        {
            writer.Write("    <rdf:Description rdf:about=\"\" xmlns=\"http://ns.adobe.com/xap/1.0/\">\n"u8);
            if (CreateDate is { } create_date) writer.Write(Encoding.UTF8.GetBytes($"      <CreateDate>{Format(create_date)}</CreateDate>\n"));
            if (CreatorTool is { }) writer.Write(Encoding.UTF8.GetBytes($"      <CreatorTool>{Escape(CreatorTool)}</CreatorTool>\n"));
            if (Identifier is { }) writer.Write(Encoding.UTF8.GetBytes($"      <Identifier>{Escape(Identifier)}</Identifier>\n"));
            if (Label is { }) writer.Write(Encoding.UTF8.GetBytes($"      <Label>{Escape(Label)}</Label>\n"));
            if (MetadataDate is { } metadata_date) writer.Write(Encoding.UTF8.GetBytes($"      <MetadataDate>{Format(metadata_date)}</MetadataDate>\n"));
            if (ModifyDate is { } modify_date) writer.Write(Encoding.UTF8.GetBytes($"      <ModifyDate>{Format(modify_date)}</ModifyDate>\n"));
            if (Rating is { }) writer.Write(Encoding.UTF8.GetBytes($"      <Rating>{Rating}</Rating>\n"));
            writer.Write("    </rdf:Description>\n"u8);
        }

        // Adobe PDF namespace
        if (Keywords is { } ||
            PDFVersion is { } ||
            Producer is { } ||
            Trapped is { })
        {
            writer.Write("    <rdf:Description rdf:about=\"\" xmlns=\"http://ns.adobe.com/pdf/1.3/\">\n"u8);
            if (Keywords is { }) writer.Write(Encoding.UTF8.GetBytes($"      <Keywords>{Escape(Keywords)}</Keywords>\n"));
            if (PDFVersion is { }) writer.Write(Encoding.UTF8.GetBytes($"      <PDFVersion>{PDFVersion / 10}.{PDFVersion % 10}</PDFVersion>\n"));
            if (Producer is { }) writer.Write(Encoding.UTF8.GetBytes($"      <Producer>{Escape(Producer)}</Producer>\n"));
            if (Trapped is { } trapped) writer.Write(Encoding.UTF8.GetBytes($"      <Trapped>{(trapped ? "True" : "False")}</Trapped>\n"));
            writer.Write("    </rdf:Description>\n"u8);
        }

        writer.Write("  </rdf:RDF>\n"u8);
        writer.Write("</xmpmeta>\n"u8);
        writer.Write("<?xpacket end=\"w\"?>\n"u8);
        writer.Flush();
    }

    public static string Escape(string s) => Regex.Replace(s, "[<>\"&\\\\]", static m => m.Value switch
    {
        "<" => "&lt;",
        ">" => "&gt;",
        "\"" => "&quot;",
        "&" => "&amp;",
        "\\" => "&#92;",
        _ => throw new(),
    });

    public static string Format(DateTime d)
    {
        switch (d.Kind)
        {
            case DateTimeKind.Local:
                var offset = TimeZoneInfo.Local.GetUtcOffset(d);
                var sign = offset >= TimeSpan.Zero ? "+" : "-";
                return $"{d:s}T{sign}{offset:hh\\:mm}";

            default:
                return $"{d:s}Z";
        }
    }
}

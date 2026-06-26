namespace Pdf.Documents;

public class XmpMetadata : PdfObject, IMetadata
{
    public XmpMetadata()
    {
        _ = Elements.TryAdd("Type", "/Metadata");
        _ = Elements.TryAdd("Subtype", "/XML");
    }

    public override void BeforeExport(PdfExportOption option)
    {
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        writer.Write("<?xpacket begin=\"\U0000FEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n"u8);
        writer.Write("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"My XMP Tool Kit v3.7\">\n"u8);
        writer.Write("  <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n"u8);
        writer.Write("    <rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\">\n"u8);
        writer.Write("      <xmp:CreateDate>2014-03-14T12:42:11+01:00</xmp:CreateDate>\n"u8);
        writer.Write("      <xmp:ModifyDate>2014-09-24T21:23:03+02:00</xmp:ModifyDate>\n"u8);
        writer.Write("      <xmp:CreatorTool>My Word Processor v10.7</xmp:CreatorTool>\n"u8);
        writer.Write("      <xmp:MetadataDate>2014-09-24T21:23:03+02:00</xmp:MetadataDate>\n"u8);
        writer.Write("    </rdf:Description>\n"u8);
        writer.Write("  </rdf:RDF>\n"u8);
        writer.Write("</x:xmpmeta>\n"u8);
        writer.Write("<?xpacket end=\"w\"?>\n"u8);
        writer.Flush();
    }
}

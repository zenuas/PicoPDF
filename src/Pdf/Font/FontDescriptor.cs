namespace PicoPDF.Pdf.Font;

public class FontDescriptor : PdfObject
{
    public string FontName { get; set; } = "";
    public FontDescriptorFlags Flags { get; set; }
    public int? MissingWidth { get; set; }

    public override void DoExport()
    {
        _ = Elements.TryAdd("Type", "/FontDescriptor");
        _ = Elements.TryAdd("FontName", $"/{FontName}");
        _ = Elements.TryAdd("Flags", (long)Flags);
        _ = Elements.TryAdd("FontBBox", new long[] { 0, 0, 0, 0 });
        _ = Elements.TryAdd("ItalicAngle", 0);
        _ = Elements.TryAdd("Ascent", 0);
        _ = Elements.TryAdd("Descent", 0);
        _ = Elements.TryAdd("CapHeight", 0);
        _ = Elements.TryAdd("StemV", 0);
        if (MissingWidth is { } x) _ = Elements.TryAdd("MissingWidth", x);
    }
}

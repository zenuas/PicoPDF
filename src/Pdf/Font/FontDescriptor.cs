namespace PicoPDF.Pdf.Font;

public class FontDescriptor : PdfObject
{
    public required string FontName { get; init; }
    public required FontDescriptorFlags Flags { get; init; }
    public int? MissingWidth { get; init; }

    public override void DoExport(PdfExportOption option)
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

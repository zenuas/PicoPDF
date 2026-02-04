using PicoPDF.Pdf.Element;

namespace PicoPDF.Pdf.Font;

public class CIDFontDictionary : PdfObject
{
    public required string Subtype { get; init; }
    public required string BaseFont { get; init; }
    public required ElementDictionary CIDSystemInfo { get; init; }
    public required FontDescriptor? FontDescriptor { get; init; }
    public int? DW { get; init; }
    public ElementArray<ElementString>? W { get; set; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/Font");
        _ = Elements.TryAdd("Subtype", $"/{Subtype}");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("CIDSystemInfo", CIDSystemInfo);
        if (FontDescriptor is { } descriptor)
        {
            RelatedObjects.Add(descriptor);
            _ = Elements.TryAdd("FontDescriptor", descriptor);
        }
        if (DW is { } dw)
        {
            _ = Elements.TryAdd("DW", dw);
        }
        if (W is { } w)
        {
            _ = Elements.TryAdd("W", w);
        }
    }
}

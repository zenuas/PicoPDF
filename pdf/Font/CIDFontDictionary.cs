using Pdf.Elements;

namespace Pdf.Font;

public class CIDFontDictionary : PdfObject
{
    public required string Subtype { get; init; }
    public required string BaseFont { get; init; }
    public required ElementDictionary CIDSystemInfo { get; init; }
    public required FontDescriptor? FontDescriptor { get; init; }
    public int? DW { get; init; }
    public ElementArray<ElementLiteral>? W { get; set; }
    public (int Margin, int Height)? DW2 { get; init; }
    public ElementArray<ElementLiteral>? W2 { get; set; }

    public override void BeforeExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/Font");
        _ = Elements.TryAdd("Subtype", $"/{Subtype}");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("CIDSystemInfo", CIDSystemInfo);
        if (FontDescriptor is { } descriptor)
        {
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
        if (DW2 is { } dw2)
        {
            _ = Elements.TryAdd("DW2", new int[] { dw2.Margin, dw2.Height });
        }
        if (W2 is { } w2)
        {
            _ = Elements.TryAdd("W2", w2);
        }
    }
}

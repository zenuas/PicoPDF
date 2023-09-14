using PicoPDF.Document.Element;

namespace PicoPDF.Document.Font;

public class CIDFontDictionary : PdfObject
{
    public string Subtype { get; init; } = "CIDFontType0";
    public string BaseFont { get; set; } = "";
    public ElementDictionary CIDSystemInfo { get; init; } = new();
    public FontDescriptor? FontDescriptor { get; set; }
    public ElementStringArray? W { get; set; } = null;

    public override void DoExport()
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
        if (W is { } w)
        {
            _ = Elements.TryAdd("W", w);
        }
    }
}

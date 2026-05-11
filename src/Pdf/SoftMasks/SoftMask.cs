using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.XObject.Form;

namespace PicoPDF.Pdf.SoftMasks;

public class SoftMask : PdfObject
{
    public required MaskMethods S { get; init; }
    public required FormXObject G { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(G);
        _ = Elements.TryAdd("Type", "/Mask");
        _ = Elements.TryAdd("S", $"/{S}");
        _ = Elements.TryAdd("G", new ElementIndirectObject() { References = G });
    }
}

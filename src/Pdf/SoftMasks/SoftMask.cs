namespace PicoPDF.Pdf.SoftMasks;

public class SoftMask : PdfObject
{
    public required MaskMethods S { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/Mask");
        _ = Elements.TryAdd("S", $"/{S}");
    }
}

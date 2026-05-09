using PicoPDF.Pdf.Drawing;

namespace PicoPDF.Pdf.XObject;

public class FormXObject : PdfObject
{
    public required (IPoint Left, IPoint Bottom, IPoint Right, IPoint Top) BBox { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Form");
        _ = Elements.TryAdd("BBox", $"[{PdfUtility.PointToString(BBox.Left.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Bottom.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Right.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Top.ToPoint(), option.PointFormat)}]");
    }
}

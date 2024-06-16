namespace PicoPDF.Pdf.Drawing;

public record struct MillimeterValue(double Value) : IPoint
{
    public readonly double ToPoint() => PdfUtility.MillimeterToPoint(Value);
}

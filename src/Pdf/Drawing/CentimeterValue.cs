namespace PicoPDF.Pdf.Drawing;

public record struct CentimeterValue(double Value) : IPoint
{
    public readonly double ToPoint() => PdfUtility.CentimeterToPoint(Value);
}

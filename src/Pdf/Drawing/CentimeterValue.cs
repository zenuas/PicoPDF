namespace PicoPDF.Pdf.Drawing;

public record struct CentimeterValue(double Value) : IPoint
{
    public double ToPoint() => PdfUtility.CentimeterToPoint(Value);
}

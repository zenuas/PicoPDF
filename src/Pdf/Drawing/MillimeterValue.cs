namespace PicoPDF.Pdf.Drawing;

public struct MillimeterValue : IPoint
{
    public required double Value;

    public double ToPoint() => PdfUtility.MillimeterToPoint(Value);
}

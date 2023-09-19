namespace PicoPDF.Pdf.Drawing;

public struct CentimeterValue : IPoint
{
    public required double Value;

    public double ToPoint() => PdfUtility.CentimeterToPoint(Value);
}

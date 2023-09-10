namespace PicoPDF.Document.Drawing;

public struct CentimeterValue : IPoint
{
    public required double Value;

    public double ToPoint() => PdfUtility.CentimeterToPoint(Value);
}

namespace PicoPDF.Pdf.Drawing;

public struct PointValue : IPoint
{
    public required double Value;

    public double ToPoint() => Value;
}

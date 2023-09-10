namespace PicoPDF.Document.Drawing;

public struct PointValue : IPoint
{
    public required double Value;

    public double ToPoint() => Value;
}

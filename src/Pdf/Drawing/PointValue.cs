namespace PicoPDF.Pdf.Drawing;

public record struct PointValue(double Value) : IPoint
{
    public readonly double ToPoint() => Value;
}

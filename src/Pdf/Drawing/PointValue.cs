namespace PicoPDF.Pdf.Drawing;

public record struct PointValue(double Value) : IPoint
{
    public double ToPoint() => Value;
}

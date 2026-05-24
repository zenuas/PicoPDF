namespace PicoPDF.Pdf.Drawing;

public record PointValue(double Value) : IPoint
{
    public double ToPoint() => Value;
}

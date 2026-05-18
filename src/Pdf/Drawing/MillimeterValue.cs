namespace PicoPDF.Pdf.Drawing;

public record struct MillimeterValue(double Value) : IPoint
{
    public readonly double ToPoint() => MillimeterToPoint(Value);

    public static double MillimeterToPoint(double v) => v * 72 / 25.4;
    public static double PointToMillimeter(double v) => v / 72 * 25.4;
}

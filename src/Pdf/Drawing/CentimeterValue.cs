namespace PicoPDF.Pdf.Drawing;

public record struct CentimeterValue(double Value) : IPoint
{
    public readonly double ToPoint() => CentimeterToPoint(Value);

    public static double CentimeterToPoint(double v) => v * 72 / 2.54;
    public static double PointToCentimeter(double v) => v / 72 * 2.54;
}

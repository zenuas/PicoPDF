namespace PicoPDF.Pdf.Drawing;

public record CentimeterValue(double Value) : IPoint
{
    public double ToPoint() => CentimeterToPoint(Value);

    public static double CentimeterToPoint(double v) => v * 72 / 2.54;
    public static double PointToCentimeter(double v) => v / 72 * 2.54;
}

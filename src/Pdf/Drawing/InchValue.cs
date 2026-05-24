using System;

namespace PicoPDF.Pdf.Drawing;

[Obsolete("use SI")]
public record InchValue(double Value) : IPoint
{
    public double ToPoint() => InchToPoint(Value);

    public static double InchToPoint(double v) => v * 72;
    public static double PointToInch(double v) => v / 72;
}

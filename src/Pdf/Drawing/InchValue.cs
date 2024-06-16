using System;

namespace PicoPDF.Pdf.Drawing;

[Obsolete("use SI")]
public record struct InchValue(double Value) : IPoint
{
    public double ToPoint() => PdfUtility.InchToPoint(Value);
}

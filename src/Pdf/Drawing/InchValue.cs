using System;

namespace PicoPDF.Pdf.Drawing;

[Obsolete("use SI")]
public record struct InchValue(double Value) : IPoint
{
    public readonly double ToPoint() => PdfUtility.InchToPoint(Value);
}

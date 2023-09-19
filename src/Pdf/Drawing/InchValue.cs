using System;

namespace PicoPDF.Pdf.Drawing;

[Obsolete("use SI")]
public struct InchValue : IPoint
{
    public required double Value;

    public double ToPoint() => PdfUtility.InchToPoint(Value);
}

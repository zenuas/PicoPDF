﻿namespace PicoPDF.Pdf.Drawing;

public record struct MillimeterValue(double Value) : IPoint
{
    public double ToPoint() => PdfUtility.MillimeterToPoint(Value);
}

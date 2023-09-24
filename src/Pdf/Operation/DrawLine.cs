using Extensions;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawLine : IOperation
{
    public required IPoint StartX { get; init; }
    public required IPoint StartY { get; init; }
    public required IPoint EndX { get; init; }
    public required IPoint EndY { get; init; }
    public IColor? Color { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        if (Color is { } c)
        {
            writer.Write($"q\n");
            writer.Write($"{c.CreateColor(true)}\n");
        }
        writer.Write($"{StartX.ToPoint()} {height - StartY.ToPoint()} m {EndX.ToPoint()} {height - EndY.ToPoint()} l S\n");
        if (Color is { })
        {
            writer.Write($"Q\n");
        }
    }
}
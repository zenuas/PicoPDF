using Mina.Extension;
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
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{LineWidth.ToPoint()} w\n");
        writer.Write($"{StartX.ToPoint()} {height - StartY.ToPoint()} m {EndX.ToPoint()} {height - EndY.ToPoint()} l S\n");
        writer.Write("Q\n");
    }
}
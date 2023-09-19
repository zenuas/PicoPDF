using Extensions;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawLine : IOperation
{
    public required IPoint StartX { get; set; }
    public required IPoint StartY { get; set; }
    public required IPoint EndX { get; set; }
    public required IPoint EndY { get; set; }
    public IColor? Color { get; init; }

    public void OperationWrite(int width, int height, Stream writer)
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
using Extensions;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawFillRectangle : IOperation
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public IColor? LineColor { get; init; }
    public IColor? FillColor { get; init; }

    public void OperationWrite(int width, int height, Stream writer)
    {
        var isstack = LineColor is { } || FillColor is { };
        if (isstack)
        {
            writer.Write($"q\n");
            if (LineColor is { } cf) writer.Write($"{cf.CreateColor(true)}\n");
            if (FillColor is { } cb) writer.Write($"{cb.CreateColor(false)}\n");
        }
        writer.Write($"{X.ToPoint()} {height - Y.ToPoint()} {Width.ToPoint()} {-Height.ToPoint()} re B\n");
        if (isstack)
        {
            writer.Write($"Q\n");
        }
    }
}
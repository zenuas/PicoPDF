using Mina.Extension;
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
    public IPoint LineWidth { get; init; } = new PointValue() { Value = 1 };

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"q\n");
        if (LineColor is { } cf) writer.Write($"{cf.CreateColor(true)}\n");
        if (FillColor is { } cb) writer.Write($"{cb.CreateColor(false)}\n");
        writer.Write($"{LineWidth.ToPoint()} w\n");
        writer.Write($"{X.ToPoint()} {height - Y.ToPoint()} {Width.ToPoint()} {-Height.ToPoint()} re B\n");
        writer.Write($"Q\n");
    }
}
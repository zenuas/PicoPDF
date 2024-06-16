using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawRectangle : IOperation
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public IColor? Color { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{LineWidth.ToPoint()} w\n");
        writer.Write($"{X.ToPoint()} {height - Y.ToPoint()} {Width.ToPoint()} {-Height.ToPoint()} re S\n");
        writer.Write($"Q\n");
    }
}
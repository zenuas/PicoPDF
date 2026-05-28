using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
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
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (LineColor is { } cf) writer.Write($"{cf.CreateColor(true)}\n");
        if (FillColor is { } cb) writer.Write($"{cb.CreateColor(false)}\n");
        writer.Write($"{LineWidth.ToPoint().ToPointString(option.PointFormat)} w\n");
        writer.Write($"{X.ToPoint().ToPointString(option.PointFormat)} {(height - Y.ToPoint()).ToPointString(option.PointFormat)} {Width.ToPoint().ToPointString(option.PointFormat)} {(-Height.ToPoint()).ToPointString(option.PointFormat)} re B\n");
        writer.Write("Q\n");
    }
}

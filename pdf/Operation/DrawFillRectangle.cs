using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

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
        writer.Write($"{LineWidth.ToPointString(option.PointFormat)} w\n");
        writer.Write($"{(X, Y).ToPointString(height, option.PointFormat)} {Width.ToPointString(option.PointFormat)} {(-Height.ToPoint()).ToPointString(option.PointFormat)} re B\n");
        writer.Write("Q\n");
    }

    public static DrawFillRectangle Create(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null) => Create(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            line,
            fill,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawFillRectangle Create(IPoint x, IPoint y, IPoint width, IPoint height, IColor? line = null, IColor? fill = null, IPoint? linewidth = null) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        LineColor = line,
        FillColor = fill,
        LineWidth = linewidth ?? new PointValue(1),
    };
}

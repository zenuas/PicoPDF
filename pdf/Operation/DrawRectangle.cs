using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

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
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{LineWidth.ToPointString(option.PointFormat)} w\n");
        writer.Write($"{(X, Y).ToPointString(height, option.PointFormat)} {Width.ToPointString(option.PointFormat)} {(-Height.ToPoint()).ToPointString(option.PointFormat)} re S\n");
        writer.Write("Q\n");
    }

    public static DrawRectangle Create(double x, double y, double width, double height, IColor? color = null, double? linewidth = null) => Create(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            color,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawRectangle Create(IPoint x, IPoint y, IPoint width, IPoint height, IColor? color = null, IPoint? linewidth = null) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        Color = color,
        LineWidth = linewidth ?? new PointValue(1),
    };
}

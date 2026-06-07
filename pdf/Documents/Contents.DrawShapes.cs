using Pdf.Drawing;
using Pdf.Operation;
using System.Linq;

namespace Pdf.Documents;

public partial class Contents
{
    public static DrawLine CreateDrawLinesOperation((double X, double Y)[] points, IColor? color = null, double? linewidth = null) => CreateDrawLinesOperation(
            [.. points.Select(x => (new PointValue(x.X), new PointValue(x.Y)))],
            color,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawLine CreateDrawLinesOperation((IPoint X, IPoint Y)[] points, IColor? color = null, IPoint? linewidth = null) => new()
    {
        Points = points,
        Color = color,
        LineWidth = linewidth ?? new PointValue(1),
    };

    public static DrawRectangle CreateDrawRectangleOperation(double x, double y, double width, double height, IColor? color = null, double? linewidth = null) => CreateDrawRectangleOperation(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            color,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawRectangle CreateDrawRectangleOperation(IPoint x, IPoint y, IPoint width, IPoint height, IColor? color = null, IPoint? linewidth = null) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        Color = color,
        LineWidth = linewidth ?? new PointValue(1),
    };

    public static DrawFillRectangle CreateDrawFillRectangleOperation(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null) => CreateDrawFillRectangleOperation(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            line,
            fill,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawFillRectangle CreateDrawFillRectangleOperation(IPoint x, IPoint y, IPoint width, IPoint height, IColor? line = null, IColor? fill = null, IPoint? linewidth = null) => new()
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

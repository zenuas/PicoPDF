using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Operation;
using System.Linq;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public void DrawLine(double start_x, double start_y, double end_x, double end_y, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawLinesOperation([(start_x, start_y), (end_x, end_y)], color, linewidth));

    public void DrawLines((double X, double Y)[] points, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawLinesOperation(points, color, linewidth));

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

    public void DrawRectangle(double x, double y, double width, double height, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawRectangleOperation(x, y, width, height, color, linewidth));

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

    public void DrawFillRectangle(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null) => Operations.Add(CreateDrawFillRectangleOperation(x, y, width, height, line, fill, linewidth));

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

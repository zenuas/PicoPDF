using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawBezierCurve : IOperation
{
    public required (IPoint X, IPoint Y) Start { get; init; }
    public required (IPoint X, IPoint Y) ControlPoint1 { get; init; }
    public required (IPoint X, IPoint Y) ControlPoint2 { get; init; }
    public required (IPoint X, IPoint Y) End { get; init; }
    public IColor? Color { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{LineWidth.ToPoint().PointToString(option.PointFormat)} w\n");
        writer.Write($"{Start.PointToString(height, option.PointFormat)} m {ControlPoint1.PointToString(height, option.PointFormat)} {ControlPoint2.PointToString(height, option.PointFormat)} {End.PointToString(height, option.PointFormat)} c S\n");
        writer.Write("Q\n");
    }
}

using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
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
        writer.Write($"{LineWidth.ToPoint()} w\n");
        writer.Write($"{PointToString(Start, height)} m {PointToString(ControlPoint1, height)} {PointToString(ControlPoint2, height)} {PointToString(End, height)} c S\n");
        writer.Write("Q\n");
    }

    public static string PointToString((IPoint X, IPoint Y) point, int height) => $"{point.X.ToPoint()} {height - point.Y.ToPoint()}";
}
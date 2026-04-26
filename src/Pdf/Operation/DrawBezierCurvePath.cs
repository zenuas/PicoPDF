using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawBezierCurvePath : IPathOperation
{
    public required (IPoint X, IPoint Y) ControlPoint1 { get; init; }
    public required (IPoint X, IPoint Y) ControlPoint2 { get; init; }
    public required (IPoint X, IPoint Y) End { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{PointToString(ControlPoint1, height)} {PointToString(ControlPoint2, height)} {PointToString(End, height)} c\n");
    }

    public static string PointToString((IPoint X, IPoint Y) point, int height) => $"{point.X.ToPoint()} {height - point.Y.ToPoint()}";
}
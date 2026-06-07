using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawBezierCurvePath : IPathOperation
{
    public required (IPoint X, IPoint Y) ControlPoint1 { get; init; }
    public required (IPoint X, IPoint Y) ControlPoint2 { get; init; }
    public required (IPoint X, IPoint Y) End { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{ControlPoint1.ToPointString(height, option.PointFormat)} {ControlPoint2.ToPointString(height, option.PointFormat)} {End.ToPointString(height, option.PointFormat)} c\n");
    }
}

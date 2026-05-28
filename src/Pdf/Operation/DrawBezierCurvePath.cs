using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawBezierCurvePath : IPathOperation
{
    public required (IPoint X, IPoint Y) ControlPoint1 { get; init; }
    public required (IPoint X, IPoint Y) ControlPoint2 { get; init; }
    public required (IPoint X, IPoint Y) End { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{ControlPoint1.PointToString(height, option.PointFormat)} {ControlPoint2.PointToString(height, option.PointFormat)} {End.PointToString(height, option.PointFormat)} c\n");
    }
}

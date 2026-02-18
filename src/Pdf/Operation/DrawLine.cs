using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf.Operation;

public class DrawLine : IOperation
{
    public required (IPoint X, IPoint Y)[] Points { get; init; }
    public IColor? Color { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{LineWidth.ToPoint()} w\n");
        writer.Write($"{PointToString(Points.First(), height)} m {Points.Skip(1).Select(x => PointToString(x, height)).Join(" l ")} l S\n");
        writer.Write("Q\n");
    }

    public static string PointToString((IPoint X, IPoint Y) point, int height) => $"{point.X.ToPoint()} {height - point.Y.ToPoint()}";
}
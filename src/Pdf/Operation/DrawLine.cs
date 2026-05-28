using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
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
        writer.Write($"{Format.PointToString(LineWidth.ToPoint(), option.PointFormat)} w\n");
        writer.Write($"{Points.First().PointToString(height, option.PointFormat)} m {Points.Skip(1).Select(x => x.PointToString(height, option.PointFormat)).Join(" l ")} l S\n");
        writer.Write("Q\n");
    }
}

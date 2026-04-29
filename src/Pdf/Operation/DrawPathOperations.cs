using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawPathOperations : IOperation
{
    public required IPathOperation[] Operations { get; init; }
    public IColor? Color { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{IOperation.PointToString(LineWidth.ToPoint(), option.PointFormat)} w\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("f*\n");
        writer.Write("Q\n");
    }
}

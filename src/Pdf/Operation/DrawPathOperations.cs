using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawPathOperations : IOperation
{
    public required string Text { get; init; }
    public required IPathOperation[] Operations { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        if (option.Debug) writer.Write($"% {Text.ReplaceLineEndings("")}\n");
        writer.Write("q\n");
        writer.Write($"{PdfUtility.PointToString(LineWidth.ToPoint(), option.PointFormat)} w\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("Q\n");
    }
}

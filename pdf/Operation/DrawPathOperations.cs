using Mina.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawPathOperations : IOperation, IHavePathOperations
{
    public required string Text { get; init; }
    public required IPathOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        if (option.Debug) writer.Write($"% {Text.ReplaceLineEndings("")}\n");
        writer.Write("q\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("Q\n");
    }
}

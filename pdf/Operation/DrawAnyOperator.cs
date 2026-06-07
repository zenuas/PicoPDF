using Mina.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawAnyOperator : IPathOperation
{
    public required string Operator { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write(Operator);
        writer.Write("\n");
    }
}

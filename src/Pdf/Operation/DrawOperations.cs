using Mina.Extension;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawOperations : IOperation
{
    public required IOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
    }
}
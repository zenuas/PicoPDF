using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawOperations : IOperation, IHaveOperations
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public required IOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
    }
}

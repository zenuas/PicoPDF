using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawClipping : IOperation, IHaveOperations
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public required IOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        writer.Write($"{(X, Y).ToPointString(height, option.PointFormat)} {Width.ToPointString(option.PointFormat)} {(-Height.ToPoint()).ToPointString(option.PointFormat)} re W n\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("Q\n");
    }
}

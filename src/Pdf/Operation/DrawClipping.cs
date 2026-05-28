using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
using System.IO;

namespace PicoPDF.Pdf.Operation;

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
        writer.Write($"{X.ToPoint().PointToString(option.PointFormat)} {(height - Y.ToPoint()).PointToString(option.PointFormat)} {Width.ToPoint().PointToString(option.PointFormat)} {(-Height.ToPoint()).PointToString(option.PointFormat)} re W n\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("Q\n");
    }
}

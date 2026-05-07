using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawRectangle : IOperation
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public IColor? Color { get; init; }
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("q\n");
        if (Color is { } c) writer.Write($"{c.CreateColor(true)}\n");
        writer.Write($"{PdfUtility.PointToString(LineWidth.ToPoint(), option.PointFormat)} w\n");
        writer.Write($"{PdfUtility.PointToString(X.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(height - Y.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(Width.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(-Height.ToPoint(), option.PointFormat)} re S\n");
        writer.Write("Q\n");
    }
}

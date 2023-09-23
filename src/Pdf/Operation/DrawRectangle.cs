using Extensions;
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

    public void OperationWrite(int width, int height, Stream writer)
    {
        if (Color is { } c)
        {
            writer.Write($"q\n");
            writer.Write($"{c.CreateColor(true)}\n");
        }
        writer.Write($"{X.ToPoint()} {height - Y.ToPoint()} {Width.ToPoint()} {-Height.ToPoint()} re S\n");
        if (Color is { })
        {
            writer.Write($"Q\n");
        }
    }
}
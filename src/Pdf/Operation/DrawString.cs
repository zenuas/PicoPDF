using Mina.Extension;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawString : IOperation
{
    public required string Text { get; init; }
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IFont Font { get; init; }
    public required double FontSize { get; init; }
    public IColor? Color { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("BT\n");
        if (Color is { } c)
        {
            writer.Write("  q\n");
            writer.Write($"  {c.CreateColor(false)}\n");
        }
        writer.Write($"  /{Font.Name} {FontSize} Tf\n");
        writer.Write($"  {X.ToPoint()} {height - Y.ToPoint()} Td\n");
        writer.Write($"  {Font.CreateTextShowingOperator(Text)}");
        if (option.Debug) writer.Write($" % {Text}");
        writer.Write("\n");
        if (Color is { })
        {
            writer.Write("  Q\n");
        }
        writer.Write("ET\n");
    }
}

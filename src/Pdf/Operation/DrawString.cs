using Extensions;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawString : IOperation
{
    public required string Text { get; set; }
    public required IPoint X { get; set; }
    public required IPoint Y { get; set; }
    public required IFont Font { get; set; }
    public required int FontSize { get; set; }
    public IColor? Color { get; init; }

    public void OperationWrite(int width, int height, Stream writer)
    {
        writer.Write($"BT\n");
        if (Color is { } c)
        {
            writer.Write($"  q\n");
            writer.Write($"  {c.CreateColor(false)}\n");
        }
        writer.Write($"  /{Font.Name} {FontSize} Tf\n");
        writer.Write($"  {X.ToPoint()} {height - Y.ToPoint()} Td\n");
        writer.Write($"  ");
        Font.CreateTextShowingOperator(Text).Each(writer.WriteByte);
        writer.Write($"\n");
        if (Color is { })
        {
            writer.Write($"  Q\n");
        }
        writer.Write($"ET\n");
    }
}

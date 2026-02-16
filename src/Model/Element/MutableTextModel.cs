using PicoPDF.Binder.Element;
using System.Drawing;

namespace PicoPDF.Model.Element;

public class MutableTextModel : ITextModel
{
    public required IElement Element { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; set; }
    public required int Size { get; init; }
    public required string[] Font { get; init; }
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public TextStyle Style { get; init; } = TextStyle.None;
    public int Width { get; init; }
    public Color? Color { get; init; }

    public override string ToString() => $"{Text}, X={X}, Y={Y}, Size={Size}";
}

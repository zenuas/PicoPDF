using PicoPDF.Binder.Element;
using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public class MutableTextModel : ITextModel
{
    public required IElement Element { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; set; }
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public TextStyle Style { get; init; } = TextStyle.None;
    public int Width { get; init; }
    public IColor? Color { get; init; }

    public override string ToString() => $"{Text}, X={X}, Y={Y}, Size={Size}";
}

using PicoPDF.Binder.Element;

namespace PicoPDF.Model.Element;

public class TextModel : IModelElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public int Width { get; init; }

    public override string ToString() => $"{Text}, X={X}, Y={Y}, Size={Size}";
}

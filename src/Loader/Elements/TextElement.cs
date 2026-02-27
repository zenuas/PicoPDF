using System.Drawing;

namespace PicoPDF.Loader.Elements;

public class TextElement : ITextElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
    public required int Size { get; init; }
    public string[] Font { get; init; } = [];
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public TextStyle Style { get; init; } = TextStyle.None;
    public int Width { get; init; }
    public int Height { get; init; }
    public Color? Color { get; init; } = null;
}

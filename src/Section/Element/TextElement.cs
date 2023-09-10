namespace PicoPDF.Section.Element;

public class TextElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
}

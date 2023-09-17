namespace PicoPDF.Section.Element;

public class TextElement : ISectionElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
    public required int Size { get; init; }
    public string Font { get; init; } = "";
}

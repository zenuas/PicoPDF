namespace PicoPDF.Model.Element;

public class TextModel : IModelElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
}

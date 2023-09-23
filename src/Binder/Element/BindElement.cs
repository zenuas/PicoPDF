namespace PicoPDF.Binder.Element;

public class BindElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; init; }
    public string Format { get; init; } = "";
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public int Width { get; init; }
}

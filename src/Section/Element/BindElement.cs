namespace PicoPDF.Section.Element;

public class BindElement : ISectionElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; init; }
    public string Format { get; init; } = "";
}

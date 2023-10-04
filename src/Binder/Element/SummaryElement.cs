namespace PicoPDF.Binder.Element;

public class SummaryElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; set; }
    public string SummaryBind { get; set; } = "";
    public string Format { get; init; } = "";
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public SummaryType SummaryType { get; init; } = SummaryType.Summary;
    public SummaryMethod SummaryMethod { get; init; } = SummaryMethod.Increment;
    public string BreakKey { get; init; } = "";
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public int Width { get; init; }
}

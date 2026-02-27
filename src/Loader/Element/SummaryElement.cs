using Binder.Data;
using System.Drawing;
using System.Globalization;

namespace PicoPDF.Loader.Element;

public class SummaryElement : ITextElement, ISummaryElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; init; }
    public string SummaryBind { get; set; } = "";
    public string SummaryCount { get; set; } = "";
    public string Format { get; init; } = "";
    public required int Size { get; init; }
    public string[] Font { get; init; } = [];
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public TextStyle Style { get; init; } = TextStyle.None;
    public int Width { get; init; }
    public int Height { get; init; }
    public Color? Color { get; init; } = null;
    public SummaryType SummaryType { get; init; } = SummaryType.Summary;
    public SummaryMethod SummaryMethod { get; init; } = SummaryMethod.Group;
    public string BreakKey { get; init; } = "";
    public object NaN { get; init; } = "NaN";
    public CultureInfo? Culture { get; init; } = null;
}

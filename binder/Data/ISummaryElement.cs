using System.Globalization;

namespace Binder.Data;

public interface ISummaryElement
{
    public string Bind { get; init; }
    public string SummaryBind { get; set; }
    public string SummaryCount { get; set; }
    public string Format { get; init; }
    public SummaryTypes SummaryType { get; init; }
    public SummaryMethods SummaryMethod { get; init; }
    public string BreakKey { get; init; }
    public object NaN { get; init; }
    public CultureInfo? Culture { get; init; }
}

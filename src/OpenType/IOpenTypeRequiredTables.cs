using System.Collections.Generic;

namespace PicoPDF.OpenType;

public interface IOpenTypeRequiredTables : IOpenTypeHeader
{
    public FontHeaderTable FontHeader { get; init; }
    public MaximumProfileTable MaximumProfile { get; init; }
    public PostScriptTable PostScript { get; init; }
    public OS2Table OS2 { get; init; }
    public HorizontalHeaderTable HorizontalHeader { get; init; }
    public HorizontalMetricsTable HorizontalMetrics { get; init; }
    public CMapTable CMap { get; init; }
    public CMapFormat4 CMap4 { get; init; }
    public List<(int Start, int End)> CMap4Range { get; init; }
    public Dictionary<char, int> CMap4Cache { get; init; }
}

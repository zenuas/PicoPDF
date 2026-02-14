using PicoPDF.OpenType.Tables;
using System;

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
    public Func<int, uint> CharToGID { get; init; }
}

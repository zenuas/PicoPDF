using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class HorizontalMetricsTable : IExportable
{
    public required HorizontalMetrics[] Metrics { get; init; }
    public required short[] LeftSideBearing { get; init; }

    public static HorizontalMetricsTable ReadFrom(Stream stream, ushort number_of_hmetrics, ushort number_of_glyphs) => new()
    {
        Metrics = Enumerable.Repeat(0, number_of_hmetrics).Select(_ => HorizontalMetrics.ReadFrom(stream)).ToArray(),
        LeftSideBearing = Enumerable.Repeat(0, number_of_glyphs - number_of_hmetrics).Select(_ => stream.ReadShortByBigEndian()).ToArray(),
    };

    public void WriteTo(Stream stream)
    {
        Metrics.Each(x => x.WriteTo(stream));
        LeftSideBearing.Each(stream.WriteShortByBigEndian);
    }
}

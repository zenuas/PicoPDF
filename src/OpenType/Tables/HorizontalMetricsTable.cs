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
        Metrics = [.. Lists.Repeat(() => HorizontalMetrics.ReadFrom(stream)).Take(number_of_hmetrics)],
        LeftSideBearing = [.. Lists.Repeat(stream.ReadShortByBigEndian).Take(number_of_glyphs - number_of_hmetrics)],
    };

    public void WriteTo(Stream stream)
    {
        Metrics.Each(x => x.WriteTo(stream));
        LeftSideBearing.Each(stream.WriteShortByBigEndian);
    }
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public record class VerticalMetricsTable
{
    public required VerticalMetrics[] Metrics { get; init; }
    public required FWORD[] TopSideBearing { get; init; }

    public static VerticalMetricsTable ReadFrom(Stream stream, ushort number_of_vmetrics, ushort number_of_glyphs) => new()
    {
        Metrics = [.. Lists.Repeat(() => VerticalMetrics.ReadFrom(stream)).Take(number_of_vmetrics)],
        TopSideBearing = [.. Lists.Repeat(stream.ReadFWORD).Take(number_of_glyphs - number_of_vmetrics)],
    };

    public void WriteTo(Stream stream)
    {
        Metrics.Each(x => x.WriteTo(stream));
        TopSideBearing.Each(stream.WriteFWORD);
    }
}

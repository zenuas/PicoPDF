using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class HorizontalMetricsTable
{
    public required HorizontalMetrics[] Metrics { get; init; }
    public required short[] LeftSideBearing { get; init; }

    public static HorizontalMetricsTable ReadFrom(Stream stream, ushort number_of_hmetrics, ushort number_of_glyphs) => new()
    {
        Metrics = Lists.RangeTo(0, number_of_hmetrics - 1).Select(_ => HorizontalMetrics.ReadFrom(stream)).ToArray(),
        LeftSideBearing = Lists.RangeTo(0, number_of_glyphs - number_of_hmetrics - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray(),
    };
}

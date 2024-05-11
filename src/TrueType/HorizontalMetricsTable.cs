using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public class HorizontalMetricsTable(Stream stream, ushort number_of_hmetrics, ushort number_of_glyphs)
{
    public readonly HorizontalMetrics[] Metrics = Lists.RangeTo(0, number_of_hmetrics - 1).Select(_ => new HorizontalMetrics(stream)).ToArray();
    public readonly short[] LeftSideBearing = Lists.RangeTo(0, number_of_glyphs - number_of_hmetrics - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray();
}

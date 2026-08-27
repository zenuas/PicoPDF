using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class PairSetTable
{
    public required ushort PairValueCount { get; init; }
    public required PairValue[] PairValueRecords { get; init; }

    public static PairSetTable ReadFrom(Stream stream, ValueFormatFlags value_format1, ValueFormatFlags value_format2)
    {
        var pair_value_count = stream.ReadUShortByBigEndian();

        var position = stream.Position;
        var pair_value_size = PairValue.LoadSize(value_format1, value_format2);

        return new()
        {
            PairValueCount = pair_value_count,
            PairValueRecords = [.. Lists.Sequence(0)
                .Select(x => PairValue.ReadFrom(stream.SeekTo(position + (x * pair_value_size)), value_format1, value_format2))
                .Take(pair_value_count)],
        };
    }
}

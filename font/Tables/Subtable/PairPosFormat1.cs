using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class PairPosFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ValueFormatFlags ValueFormat1 { get; init; }
    public required ValueFormatFlags ValueFormat2 { get; init; }
    public required ushort PairSetCount { get; init; }
    public required Offset16[] PairSetOffsets { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required PairSetTable[] PairSets { get; init; }

    public static PairPosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var value_format1 = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var value_format2 = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var pair_set_count = stream.ReadUShortByBigEndian();
        var pair_set_offsets = Lists.Repeat(stream.ReadOffset16).Take(pair_set_count).ToArray();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            ValueFormat1 = value_format1,
            ValueFormat2 = value_format2,
            PairSetCount = pair_set_count,
            PairSetOffsets = pair_set_offsets,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            PairSets = [.. pair_set_offsets.Select(x => PairSetTable.ReadFrom(stream.SeekTo(position + x.Value), value_format1, value_format2))],
        };
    }
}

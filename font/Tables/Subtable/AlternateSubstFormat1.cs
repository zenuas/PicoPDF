using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class AlternateSubstFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ushort AlternateSetCount { get; init; }
    public required Offset16[] AlternateSetOffsets { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required AlternateSetTable[] AlternateSets { get; init; }

    public static AlternateSubstFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var alternate_set_count = stream.ReadUShortByBigEndian();
        var alternate_set_offsets = Lists.Repeat(stream.ReadOffset16).Take(alternate_set_count).ToArray();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            AlternateSetCount = alternate_set_count,
            AlternateSetOffsets = alternate_set_offsets,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            AlternateSets = [.. alternate_set_offsets.Select(x => AlternateSetTable.ReadFrom(stream.SeekTo(position + x.Value)))]
        };
    }
}

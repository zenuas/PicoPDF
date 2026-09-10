using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class MultipleSubstFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ushort SequenceCount { get; init; }
    public required Offset16[] SequenceOffsets { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required SequenceTable[] Sequences { get; init; }

    public static MultipleSubstFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);
        var coverage_offset = stream.ReadOffset16();
        var sequence_count = stream.ReadUShortByBigEndian();
        var sequence_offsets = Lists.Repeat(stream.ReadOffset16).Take(sequence_count).ToArray();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            SequenceCount = sequence_count,
            SequenceOffsets = sequence_offsets,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            Sequences = [.. sequence_offsets.Select(x => SequenceTable.ReadFrom(stream.SeekTo(position + x.Value)))]
        };
    }
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class LigatureSubstFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ushort LigSetCount { get; init; }
    public required Offset16[] LigatureSetOffsets { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required LigatureSetTable[] LigatureSet { get; init; }

    public static LigatureSubstFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var lig_set_count = stream.ReadUShortByBigEndian();
        var ligature_set_offsets = Lists.Repeat(stream.ReadOffset16).Take(lig_set_count).ToArray();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            LigSetCount = lig_set_count,
            LigatureSetOffsets = ligature_set_offsets,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            LigatureSet = [.. ligature_set_offsets.Select(x => LigatureSetTable.ReadFrom(stream.SeekTo(position + x.Value)))],
        };
    }
}

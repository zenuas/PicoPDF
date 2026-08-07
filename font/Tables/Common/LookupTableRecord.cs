using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class LookupTableRecord
{
    public required ushort LookupType { get; init; }
    public required ushort LookupFlag { get; init; }
    public required ushort SubTableCount { get; init; }
    public required Offset16[] SubtableOffsets { get; init; }
    public required ushort MarkFilteringSet { get; init; }

    public static LookupTableRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var lookup_type = stream.ReadUShortByBigEndian();
        var lookup_flag = stream.ReadUShortByBigEndian();
        var subtable_count = stream.ReadUShortByBigEndian();
        var subtable_offsets = Lists.Repeat(stream.ReadOffset16).Take(subtable_count).ToArray();
        var mark_filtering_set = stream.ReadUShortByBigEndian();

        return new()
        {
            LookupType = lookup_type,
            LookupFlag = lookup_flag,
            SubTableCount = subtable_count,
            SubtableOffsets = subtable_offsets,
            MarkFilteringSet = mark_filtering_set,
        };
    }

    public int SizeOf() =>
        LookupType.SizeOf() +
        LookupFlag.SizeOf() +
        SubTableCount.SizeOf() +
        (Offset16.SizeOf() * SubtableOffsets.Length) +
        MarkFilteringSet.SizeOf();
}

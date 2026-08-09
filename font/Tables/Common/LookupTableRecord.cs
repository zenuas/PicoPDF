using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables.GlyphSubstitution;
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
    public required ISubtable[] Subtables { get; init; }

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
            Subtables = [.. subtable_offsets.Select<Offset16, ISubtable>(offset =>
            {
                _ = stream.Seek(position + offset.Value, SeekOrigin.Begin);
                var format = stream.ReadUShortByBigEndian();
                return ((lookup_type * 10) + format) switch
                {
                    1_1 => SingleSubstFormat1.ReadFrom(stream),
                    1_2 => SingleSubstFormat2.ReadFrom(stream),
                    2_1 => MultipleSubstFormat1.ReadFrom(stream),
                    3_1 => AlternateSubstFormat1.ReadFrom(stream),
                    4_1 => LigatureSubstFormat1.ReadFrom(stream),
                    5_1 => SequenceContextFormat1.ReadFrom(stream),
                    5_2 => SequenceContextFormat2.ReadFrom(stream),
                    5_3 => SequenceContextFormat3.ReadFrom(stream),
                    6_1 => ChainedSequenceContextFormat1.ReadFrom(stream),
                    6_2 => ChainedSequenceContextFormat2.ReadFrom(stream),
                    6_3 => ChainedSequenceContextFormat3.ReadFrom(stream),
                    7_1 => SubstExtensionFormat1.ReadFrom(stream),
                    8_1 => ReverseChainSingleSubstFormat1.ReadFrom(stream),
                    _ => throw new(),
                };
            })],
        };
    }

    public int SizeOf() =>
        LookupType.SizeOf() +
        LookupFlag.SizeOf() +
        SubTableCount.SizeOf() +
        (Offset16.SizeOf() * SubtableOffsets.Length) +
        MarkFilteringSet.SizeOf();
}

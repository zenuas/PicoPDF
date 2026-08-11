using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables.Subtable;
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

    public static LookupTableRecord ReadFrom(Stream stream, TableTypes table_type)
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
                return (((int)table_type * 100) + (lookup_type * 10) + format) switch
                {
                    // GPOS
                    1_1_1 => SinglePosFormat1.ReadFrom(stream),
                    1_1_2 => SinglePosFormat2.ReadFrom(stream),
                    1_2_1 => PairPosFormat1.ReadFrom(stream),
                    1_2_2 => PairPosFormat2.ReadFrom(stream),
                    1_3_1 => CursivePosFormat1.ReadFrom(stream),
                    1_4_1 => MarkBasePosFormat1.ReadFrom(stream),
                    1_5_1 => MarkLigPosFormat1.ReadFrom(stream),
                    1_6_1 => MarkMarkPosFormat1.ReadFrom(stream),
                    1_7_1 => SequenceContextFormat1.ReadFrom(stream),
                    1_7_2 => SequenceContextFormat2.ReadFrom(stream),
                    1_7_3 => SequenceContextFormat3.ReadFrom(stream),
                    1_8_1 => ChainedSequenceContextFormat1.ReadFrom(stream),
                    1_8_2 => ChainedSequenceContextFormat2.ReadFrom(stream),
                    1_8_3 => ChainedSequenceContextFormat3.ReadFrom(stream),
                    1_9_1 => PosExtensionFormat1.ReadFrom(stream),

                    // GSUB
                    2_1_1 => SingleSubstFormat1.ReadFrom(stream),
                    2_1_2 => SingleSubstFormat2.ReadFrom(stream),
                    2_2_1 => MultipleSubstFormat1.ReadFrom(stream),
                    2_3_1 => AlternateSubstFormat1.ReadFrom(stream),
                    2_4_1 => LigatureSubstFormat1.ReadFrom(stream),
                    2_5_1 => SequenceContextFormat1.ReadFrom(stream),
                    2_5_2 => SequenceContextFormat2.ReadFrom(stream),
                    2_5_3 => SequenceContextFormat3.ReadFrom(stream),
                    2_6_1 => ChainedSequenceContextFormat1.ReadFrom(stream),
                    2_6_2 => ChainedSequenceContextFormat2.ReadFrom(stream),
                    2_6_3 => ChainedSequenceContextFormat3.ReadFrom(stream),
                    2_7_1 => SubstExtensionFormat1.ReadFrom(stream),
                    2_8_1 => ReverseChainSingleSubstFormat1.ReadFrom(stream),

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

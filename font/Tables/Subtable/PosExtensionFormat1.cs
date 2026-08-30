using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class PosExtensionFormat1 : ISubtable, ISinglePosition, IPairPosition
{
    public required ushort Format { get; init; }
    public required ushort ExtensionLookupType { get; init; }
    public required Offset32 ExtensionOffset { get; init; }
    public required ISubtable Extension { get; init; }

    public static PosExtensionFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var extension_lookup_type = stream.ReadUShortByBigEndian();
        var extension_offset = stream.ReadOffset32();

        _ = stream.Seek(position + extension_offset.Value, SeekOrigin.Begin);
        var format = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 1,
            ExtensionLookupType = extension_lookup_type,
            ExtensionOffset = extension_offset,
            Extension = ((extension_lookup_type * 10) + format) switch
            {
                // GPOS
                1_1 => SinglePosFormat1.ReadFrom(stream),
                1_2 => SinglePosFormat2.ReadFrom(stream),
                2_1 => PairPosFormat1.ReadFrom(stream),
                2_2 => PairPosFormat2.ReadFrom(stream),
                3_1 => CursivePosFormat1.ReadFrom(stream),
                4_1 => MarkBasePosFormat1.ReadFrom(stream),
                5_1 => MarkLigPosFormat1.ReadFrom(stream),
                6_1 => MarkMarkPosFormat1.ReadFrom(stream),
                7_1 => SequenceContextFormat1.ReadFrom(stream),
                7_2 => SequenceContextFormat2.ReadFrom(stream),
                7_3 => SequenceContextFormat3.ReadFrom(stream),
                8_1 => ChainedSequenceContextFormat1.ReadFrom(stream),
                8_2 => ChainedSequenceContextFormat2.ReadFrom(stream),
                8_3 => ChainedSequenceContextFormat3.ReadFrom(stream),
                9_1 => PosExtensionFormat1.ReadFrom(stream),

                _ => throw new(),
            },
        };
    }

    public ValueRecord? GetPosition(uint gid) => Extension is ISinglePosition singlepos ? singlepos.GetPosition(gid) : null;

    public (ValueRecord First, ValueRecord Second)? GetPosition(uint first_gid, uint second_gid) => Extension is IPairPosition pairpos ? pairpos.GetPosition(first_gid, second_gid) : null;
}

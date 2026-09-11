using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class MarkRecord
{
    public required ushort MarkClass { get; init; }
    public required Offset16 MarkAnchorOffset { get; init; }
    public required IAnchorFormat MarkAnchor { get; init; }

    public static MarkRecord ReadFrom(Stream stream, long mark_array_table_offset)
    {
        var mark_class = stream.ReadUShortByBigEndian();
        var mark_anchor_offset = stream.ReadOffset16();

        return new()
        {
            MarkClass = mark_class,
            MarkAnchorOffset = mark_anchor_offset,
            // Offset to Anchor table, from beginning of MarkArray table.
            MarkAnchor = IAnchorFormat.ReadFrom(stream.SeekTo(mark_array_table_offset + mark_anchor_offset.Value)),
        };
    }

    public static int SizeOf() => /* sizeof(MarkClass) */sizeof(ushort) + /* sizeof(MarkAnchorOffset) */Offset16.SizeOf();
}

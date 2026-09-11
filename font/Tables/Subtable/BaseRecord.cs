using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class BaseRecord
{
    public required Offset16[] BaseAnchorOffsets { get; init; }
    public required IAnchorFormat?[] BaseAnchors { get; init; }

    public static BaseRecord ReadFrom(Stream stream, long base_array_table_offset, ushort mark_class_count)
    {
        var base_anchor_offsets = Lists.Repeat(stream.ReadOffset16).Take(mark_class_count).ToArray();

        return new()
        {
            BaseAnchorOffsets = base_anchor_offsets,
            // Array of offsets (one per mark class) to Anchor tables. Offsets are from beginning of BaseArray table, ordered by class (offsets may be NULL).
            BaseAnchors = [.. base_anchor_offsets.Select(x => x.Value == 0 ? null : IAnchorFormat.ReadFrom(stream.SeekTo(base_array_table_offset + x.Value)))],
        };
    }

    public static int SizeOf(ushort mark_class_count) => /* sizeof(BaseAnchorOffsets) */Offset16.SizeOf() * mark_class_count;
}

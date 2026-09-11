using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class EntryExitRecord
{
    public required Offset16 EntryAnchorOffset { get; init; }
    public required Offset16 ExitAnchorOffset { get; init; }
    public required IAnchorFormat? EntryAnchor { get; init; }
    public required IAnchorFormat? ExitAnchor { get; init; }

    public static EntryExitRecord ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var entry_anchor_offset = stream.ReadOffset16();
        var exit_anchor_offset = stream.ReadOffset16();

        return new()
        {
            EntryAnchorOffset = entry_anchor_offset,
            ExitAnchorOffset = exit_anchor_offset,
            EntryAnchor = entry_anchor_offset.Value == 0 ? null : IAnchorFormat.ReadFrom(stream.SeekTo(position + entry_anchor_offset.Value)),
            ExitAnchor = exit_anchor_offset.Value == 0 ? null : IAnchorFormat.ReadFrom(stream.SeekTo(position + exit_anchor_offset.Value)),
        };
    }

    public static int SizeOf() => /* sizeof(EntryAnchorOffset) */Offset16.SizeOf() +/* sizeof(ExitAnchorOffset) */Offset16.SizeOf();
}

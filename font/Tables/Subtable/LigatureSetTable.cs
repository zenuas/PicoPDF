using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class LigatureSetTable
{
    public required ushort LigatureCount { get; init; }
    public required Offset16[] LigatureOffsets { get; init; }
    public required LigatureTable[] Ligature { get; init; }

    public static LigatureSetTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var ligature_count = stream.ReadUShortByBigEndian();
        var ligature_offsets = Lists.Repeat(stream.ReadOffset16).Take(ligature_count).ToArray();

        return new()
        {
            LigatureCount = ligature_count,
            LigatureOffsets = ligature_offsets,
            Ligature = [.. ligature_offsets.Select(x => LigatureTable.ReadFrom(stream.SeekTo(position + x.Value)))],
        };
    }
}

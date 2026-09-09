using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class LigatureTable
{
    public required ushort LigGlyph { get; init; }
    public required ushort CompCount { get; init; }
    public required ushort[] Component { get; init; }

    public static LigatureTable ReadFrom(Stream stream)
    {
        var lig_glyph = stream.ReadUShortByBigEndian();
        var comp_count = stream.ReadUShortByBigEndian();

        return new()
        {
            LigGlyph = lig_glyph,
            CompCount = comp_count,
            Component = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(comp_count)],
        };
    }
}

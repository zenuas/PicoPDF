using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class AlternateSetTable
{
    public required ushort GlyphCount { get; init; }
    public required ushort[] AlternateGlyphIDs { get; init; }

    public static AlternateSetTable ReadFrom(Stream stream)
    {
        var glyph_count = stream.ReadUShortByBigEndian();

        return new()
        {
            GlyphCount = glyph_count,
            AlternateGlyphIDs = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count)],
        };
    }
}

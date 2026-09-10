using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class SequenceTable
{
    public required ushort GlyphCount { get; init; }
    public required ushort[] SubstituteGlyphIDs { get; init; }

    public static SequenceTable ReadFrom(Stream stream)
    {
        var glyph_count = stream.ReadUShortByBigEndian();

        return new()
        {
            GlyphCount = glyph_count,
            SubstituteGlyphIDs = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count)],
        };
    }
}

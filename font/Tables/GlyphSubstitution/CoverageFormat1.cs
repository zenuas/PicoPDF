using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.GlyphSubstitution;

public class CoverageFormat1 : ISubtable, ICoverageFormat
{
    public required ushort Format { get; init; }
    public required ushort GlyphCount { get; init; }
    public required ushort[] GlyphArray { get; init; }

    public static CoverageFormat1 ReadFrom(Stream stream)
    {
        var glyph_count = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 1,
            GlyphCount = glyph_count,
            GlyphArray = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count)],
        };
    }
}

using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class ClassDefFormat1 : ISubtable, IClassDefFormat
{
    public required ushort Format { get; init; }
    public required ushort StartGlyphID { get; init; }
    public required ushort GlyphCount { get; init; }
    public required ushort[] ClassValues { get; init; }

    public static ClassDefFormat1 ReadFrom(Stream stream)
    {
        var start_gid = stream.ReadUShortByBigEndian();
        var glyph_count = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 1,
            StartGlyphID = start_gid,
            GlyphCount = glyph_count,
            ClassValues = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count)],
        };
    }

    // Any glyph not included in the range of covered glyph IDs is assigned to Class 0.
    public ushort GetClassValue(uint gid) => StartGlyphID <= gid && gid < StartGlyphID + GlyphCount ? ClassValues[gid - StartGlyphID] : (ushort)0;
}

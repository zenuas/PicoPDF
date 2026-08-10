using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.GlyphSubstitution;

public class SingleSubstFormat2 : ISubtable, ISingleConvert
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ushort GlyphCount { get; init; }
    public required ushort[] SubstituteGlyphIDs { get; init; }
    public required ICoverageFormat Coverage { get; init; }

    public static SingleSubstFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var glyph_count = stream.ReadUShortByBigEndian();
        var substitute_glyph_ids = Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count).ToArray();

        _ = stream.Seek(position + coverage_offset.Value, SeekOrigin.Begin);
        var coverage_format = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 2,
            CoverageOffset = coverage_offset,
            GlyphCount = glyph_count,
            SubstituteGlyphIDs = substitute_glyph_ids,
            Coverage = coverage_format switch
            {
                1 => CoverageFormat1.ReadFrom(stream),
                2 => CoverageFormat2.ReadFrom(stream),
                _ => throw new(),
            },
        };
    }

    public uint? Convert(uint gid) => Coverage.FindOrNull(gid) is { } index ? SubstituteGlyphIDs[index] : null;
}

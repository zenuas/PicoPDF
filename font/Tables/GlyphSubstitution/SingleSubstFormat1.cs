using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class SingleSubstFormat1 : ISubtable, ISingleConvert
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required short DeltaGlyphID { get; init; }
    public required ICoverageFormat Coverage { get; init; }

    public static SingleSubstFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var delta_glyph_id = stream.ReadShortByBigEndian();

        _ = stream.Seek(position + coverage_offset.Value, SeekOrigin.Begin);
        var coverage_format = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            DeltaGlyphID = delta_glyph_id,
            Coverage = coverage_format switch
            {
                1 => CoverageFormat1.ReadFrom(stream),
                2 => CoverageFormat2.ReadFrom(stream),
                _ => throw new(),
            },
        };
    }

    public uint? Convert(uint gid) => Coverage.FindOrNull(gid) is { } ? (uint)((gid + DeltaGlyphID) & 0xFFFF) : null;
}

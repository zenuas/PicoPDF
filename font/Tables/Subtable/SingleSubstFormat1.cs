using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

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

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            DeltaGlyphID = delta_glyph_id,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
        };
    }

    // Addition of deltaGlyphID is modulo 65536. 
    // If the result after adding deltaGlyphID to the input glyph index is less than zero, add 65536 to obtain a valid glyph ID.
    public uint? Convert(uint gid) => Coverage.FindOrNull(gid) is { } ? (uint)((gid + DeltaGlyphID) & 0xFFFF) : null;
}

using OpenType.Extension;

namespace OpenType.Tables.TrueType;

public class CompositeGlyphMatrix2x2Record : ICompositeGlyphRecord
{
    public required CompositeGlyphFlags Flags { get; init; }
    public required ushort GlyphIndex { get; init; }
    public required int Argument1 { get; init; }
    public required int Argument2 { get; init; }
    public required F2DOT14 XScale { get; init; }
    public required F2DOT14 Scale01 { get; init; }
    public required F2DOT14 Scale10 { get; init; }
    public required F2DOT14 YScale { get; init; }
}

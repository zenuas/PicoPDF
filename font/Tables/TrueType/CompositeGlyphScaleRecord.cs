using OpenType.Extension;

namespace OpenType.Tables.TrueType;

public class CompositeGlyphScaleRecord : ICompositeGlyphRecord
{
    public required CompositeGlyphFlags Flags { get; init; }
    public required ushort GlyphIndex { get; init; }
    public required int Argument1 { get; init; }
    public required int Argument2 { get; init; }
    public required F2DOT14 Scale { get; init; }
}

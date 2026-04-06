namespace OpenType.Tables.TrueType;

public interface ICompositeGlyphRecord
{
    public CompositeGlyphFlags Flags { get; init; }
    public ushort GlyphIndex { get; init; }
    public int Argument1 { get; init; }
    public int Argument2 { get; init; }
}

namespace PicoPDF.OpenType.Tables.TrueType;

public class CompositeGlyphRecord
{
    public required ushort Flags { get; init; }
    public required ushort GlyphIndex { get; init; }
    public required int Argument1 { get; init; }
    public required int Argument2 { get; init; }
    public required ushort Scale { get; init; }
    public required ushort XScale { get; init; }
    public required ushort YScale { get; init; }
    public required ushort Scale01 { get; init; }
    public required ushort Scale10 { get; init; }
}

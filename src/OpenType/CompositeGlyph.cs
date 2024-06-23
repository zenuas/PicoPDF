namespace PicoPDF.OpenType;

public class CompositeGlyph : ITrueTypeGlyph
{
    public required short NumberOfContours { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
}

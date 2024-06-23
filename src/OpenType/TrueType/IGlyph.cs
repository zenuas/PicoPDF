namespace PicoPDF.OpenType.TrueType;

public interface IGlyph
{
    public short NumberOfContours { get; init; }
    public short XMin { get; init; }
    public short YMin { get; init; }
    public short XMax { get; init; }
    public short YMax { get; init; }
}

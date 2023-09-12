namespace PicoPDF.Document.Font.TrueType;

public class TrueTypeFont
{
    public required string FontFamily { get; init; }
    public required string Style { get; init; }
    public required string FullFontName { get; init; }
    public required string PostScriptName { get; init; }
    public required FontHeaderTable FontHeader { get; init; }
}

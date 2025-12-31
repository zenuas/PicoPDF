namespace PicoPDF.OpenType;

public class FontPath : IFontPath
{
    public required string Path { get; init; }
    public required bool ForceEmbedded { get; init; }
}

namespace PicoPDF.OpenType;

public class FontCollectionPath : IFontPath
{
    public required string Path { get; init; }
    public required int Index { get; init; }
    public required bool ForceEmbedded { get; init; }
}

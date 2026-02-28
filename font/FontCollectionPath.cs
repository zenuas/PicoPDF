namespace OpenType;

public class FontCollectionPath : IFontPath
{
    public required string Path { get; init; }
    public required int Index { get; init; }
}

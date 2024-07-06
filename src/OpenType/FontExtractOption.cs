namespace PicoPDF.OpenType;

public class FontExtractOption
{
    public required char[] ExtractChars { get; init; }
    public (Platforms? PlatformID, Encodings? EncodingID, ushort? LanguageID, NameIDs? NameID)[] OutputNames { get; init; } = [];
    public (Platforms PlatformID, Encodings EncodingID)[] OutputCMap { get; init; } = [];
}

namespace PicoPDF.OpenType;

public class FontExtractOption
{
    public required char[] ExtractChars { get; init; }
    public (Platforms? PlatformID, Encodings? EncodingID, ushort? LanguageID, NameIDs? NameID)[] OutputNames { get; init; } = [
        new(null, null, null, NameIDs.FontFamilyName),
        new(null, null, null, NameIDs.FontSubfamilyName),
        new(null, null, null, NameIDs.UniqueFontIdentifier),
        new(null, null, null, NameIDs.FullFontName),
    ];
    public (Platforms PlatformID, Encodings EncodingID)[] OutputCMap { get; init; } = [
        new(Platforms.Unicode, Encodings.Unicode2_0_BMPOnly),
        new(Platforms.Windows, Encodings.Windows_UnicodeBMP),
    ];
}

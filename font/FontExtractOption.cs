namespace OpenType;

public class FontExtractOption
{
    public required int[] ExtractChars { get; init; }
    public (Platforms? PlatformID, Encodings? EncodingID, ushort? LanguageID, NameIDs? NameID)[] OutputNames { get; init; } = [
        new(null, null, null, NameIDs.FontFamilyName),
        new(null, null, null, NameIDs.FontSubfamilyName),
        new(null, null, null, NameIDs.UniqueFontIdentifier),
        new(null, null, null, NameIDs.FullFontName),
        new(null, null, null, NameIDs.Version),
        new(null, null, null, NameIDs.PostScriptName),
    ];
    public (Platforms PlatformID, Encodings EncodingID, CMapFormats CMapFormat)[] OutputCMap { get; init; } = [
        new(Platforms.Unicode, Encodings.Unicode2_0_BMPOnly, CMapFormats.Format4),
        new(Platforms.Unicode, Encodings.Unicode2_0_FullRepertoire, CMapFormats.Format12),
        new(Platforms.Windows, Encodings.Windows_UnicodeBMP, CMapFormats.Format4),
        new(Platforms.Windows, Encodings.Windows_UnicodeFullRepertoire, CMapFormats.Format12),
    ];
}

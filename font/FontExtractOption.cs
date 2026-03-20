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
        new(Platforms.Unicode, Encodings.Unicode2_0_FullRepertoire, CMapFormats.Format12),

        // Fonts that support only Unicode BMP characters (U+0000 to U+FFFF) on the Windows platform must use encoding 1 with a format 4 subtable.
        // This encoding must not be used to support Unicode supplementary-plane characters.
        // Fonts that support Unicode supplementary-plane characters (U+10000 to U+10FFFF) on the Windows platform must use encoding 10 with a format 12 subtable.
        //  https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#windows-platform-platform-id--3
        new(Platforms.Windows, Encodings.Windows_UnicodeBMP, CMapFormats.Format4),
        new(Platforms.Windows, Encodings.Windows_UnicodeFullRepertoire, CMapFormats.Format12),
    ];
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Text;

namespace OpenType.Tables;

public class NameRecord
{
    public required ushort PlatformID { get; init; }
    public required ushort EncodingID { get; init; }
    public required ushort LanguageID { get; init; }
    public required ushort NameID { get; init; }
    public required ushort Length { get; init; }
    public required ushort Offset { get; init; }

    public static NameRecord ReadFrom(Stream stream) => new()
    {
        PlatformID = stream.ReadUShortByBigEndian(),
        EncodingID = stream.ReadUShortByBigEndian(),
        LanguageID = stream.ReadUShortByBigEndian(),
        NameID = stream.ReadUShortByBigEndian(),
        Length = stream.ReadUShortByBigEndian(),
        Offset = stream.ReadOffset16(),
    };

    public Encoding GetEncoding() => (PlatformID, EncodingID, LanguageID) switch
    {
        ((ushort)Platforms.Unicode, _, _) => Encoding.BigEndianUnicode,
        ((ushort)Platforms.Macintosh, _, _) => Encoding.UTF8,

        // If a font has records for encoding IDs 3, 4 or 5, the corresponding string data should be encoded using code pages 936, 950 and 949, respectively.
        // Otherwise, all string data for platform 3 must be encoded in UTF-16BE.
        //  https://learn.microsoft.com/en-us/typography/opentype/spec/name#windows-encoding-ids
        ((ushort)Platforms.Windows, 3, _) => Encoding.GetEncodingOrUtf8(936),
        ((ushort)Platforms.Windows, 4, _) => Encoding.GetEncodingOrUtf8(950),
        ((ushort)Platforms.Windows, 5, _) => Encoding.GetEncodingOrUtf8(949),
        ((ushort)Platforms.Windows, _, _) => Encoding.BigEndianUnicode,

        _ => Encoding.UTF8,
    };

    public int SizeOf() => PlatformID.SizeOf() + EncodingID.SizeOf() + LanguageID.SizeOf() + NameID.SizeOf() + Length.SizeOf() + Offset.SizeOf();

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}, LanguageID={LanguageID}, NameID={NameID}";
}

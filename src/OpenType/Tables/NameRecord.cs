using Mina.Extension;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType.Tables;

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
        Offset = stream.ReadUShortByBigEndian(),
    };

    public Encoding GetEncoding() => (PlatformID, EncodingID, LanguageID) switch
    {
        ((ushort)Platforms.Unicode, _, _) => Encoding.BigEndianUnicode,
        ((ushort)Platforms.Windows, 0 or 1 or 10, _) => Encoding.BigEndianUnicode,
        ((ushort)Platforms.Windows, 2, _) => Encoding.GetEncodingOrUtf8("shift_jis"),
        ((ushort)Platforms.Windows, 4, _) => Encoding.GetEncodingOrUtf8("big5"),
        ((ushort)Platforms.Windows, 5, _) => Encoding.GetEncodingOrUtf8("x-cp20949"),
        ((ushort)Platforms.Windows, 6, _) => Encoding.GetEncodingOrUtf8("Johab"),
        ((ushort)Platforms.Macintosh, 1, _) => Encoding.GetEncodingOrUtf8("shift_jis"),
        _ => Encoding.UTF8,
    };

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}, LanguageID={LanguageID}, NameID={NameID}";
}

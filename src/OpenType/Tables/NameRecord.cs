using Mina.Extension;
using System.IO;

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

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}, LanguageID={LanguageID}, NameID={NameID}";
}

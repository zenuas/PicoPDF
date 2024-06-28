using Mina.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.OpenType;

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

    public static (string Name, NameRecord NameRecord)[] ReadFrom(Stream stream, TableRecord rec)
    {
        var bytes = stream.ReadPositionBytes(rec.Offset, (int)rec.Length);
        var buffer = new MemoryStream(bytes);
        var nametable = NameTable.ReadFrom(buffer);
        return Enumerable.Range(0, nametable.Count)
            .Select(_ => ReadFrom(buffer))
            .Select(x => ((x.PlatformID == 0 || x.PlatformID == 3 ? Encoding.BigEndianUnicode : Encoding.UTF8).GetString(bytes, nametable.StringOffset + x.Offset, x.Length), x))
            .ToArray();
    }

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}, LanguageID={LanguageID}, NameID={NameID}";
}

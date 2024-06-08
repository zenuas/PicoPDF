using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.OpenType;

public class NameRecord(Stream stream)
{
    public readonly ushort PlatformID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort EncodingID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort LanguageID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort NameID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort Length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort Offset = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

    public static (string Name, NameRecord NameRecord)[] ReadFrom(Stream stream, TableRecord rec)
    {
        var bytes = stream.ReadPositionBytes(rec.Offset, (int)rec.Length);
        var buffer = new MemoryStream(bytes);
        var nametable = new NameTable(buffer);
        return Enumerable.Range(0, nametable.Count)
            .Select(_ => new NameRecord(buffer))
            .Select(x => ((x.PlatformID == 0 || x.PlatformID == 3 ? Encoding.BigEndianUnicode : Encoding.UTF8).GetString(bytes, nametable.StringOffset + x.Offset, x.Length), x))
            .ToArray();
    }

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}, LanguageID={LanguageID}, NameID={NameID}";
}

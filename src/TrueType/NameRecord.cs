using Extensions;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.TrueType;

public struct NameRecord
{
    public ushort PlatformID;
    public ushort EncodingID;
    public ushort LanguageID;
    public ushort NameID;
    public ushort Length;
    public ushort Offset;

    public static NameRecord ReadFrom(Stream stream) => new()
    {
        PlatformID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        EncodingID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        LanguageID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NameID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Offset = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
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

using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public class EncodingRecord(Stream stream)
{
    public readonly ushort PlatformID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort EncodingID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly uint Offset = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));

    public static EncodingRecord[] ReadFrom(Stream stream, TableRecord rec)
    {
        var bytes = stream.ReadPositionBytes(rec.Offset, (int)rec.Length);
        var buffer = new MemoryStream(bytes);
        var cmaptable = new CMapTable(buffer);
        return Enumerable.Range(0, cmaptable.NumberOfTables)
            .Select(_ => new EncodingRecord(buffer))
            .ToArray();
    }

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}";
}

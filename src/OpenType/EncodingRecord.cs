using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class EncodingRecord
{
    public required ushort PlatformID { get; init; }
    public required ushort EncodingID { get; init; }
    public required uint Offset { get; init; }

    public PlatformEncodings PlatformEncoding { get => (PlatformEncodings)(((uint)PlatformID << 16) | EncodingID); }

    public static EncodingRecord ReadFrom(Stream stream) => new()
    {
        PlatformID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        EncodingID = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Offset = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
    };

    public static EncodingRecord[] ReadFrom(Stream stream, TableRecord rec)
    {
        var bytes = stream.ReadPositionBytes(rec.Offset, (int)rec.Length);
        var buffer = new MemoryStream(bytes);
        var cmaptable = CMapTable.ReadFrom(buffer);
        return Enumerable.Range(0, cmaptable.NumberOfTables)
            .Select(_ => ReadFrom(buffer))
            .ToArray();
    }

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}";
}

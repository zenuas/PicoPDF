using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class OffsetTable
{
    public required uint Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required ushort SearchRange { get; init; }
    public required ushort EntrySelector { get; init; }
    public required ushort RangeShift { get; init; }

    public static OffsetTable ReadFrom(Stream stream) => new()
    {
        Version = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
    };
}

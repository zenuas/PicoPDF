using Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct OffsetTable
{
    public uint Version;
    public ushort NumberOfTables;
    public ushort SearchRange;
    public ushort EntrySelector;
    public ushort RangeShift;

    public static OffsetTable ReadFrom(Stream stream) => new()
    {
        Version = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))
    };
}

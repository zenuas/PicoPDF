using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class OffsetTable(Stream stream)
{
    public readonly uint Version = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly ushort NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
}

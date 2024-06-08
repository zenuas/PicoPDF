using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class HorizontalHeaderTable(Stream stream)
{
    public readonly ushort MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly short Ascender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short Descender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short LineGap = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort AdvanceWidthMax = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly short MinLeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short MinRightSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short XMaxExtent = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short CaretSlopeRise = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short CaretSlopeRun = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short CaretOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short Reserved1 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short Reserved2 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short Reserved3 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short Reserved4 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short MetricDataFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort NumberOfHMetrics = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
}

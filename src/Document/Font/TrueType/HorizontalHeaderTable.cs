using Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.Document.Font.TrueType;

public struct HorizontalHeaderTable
{
    public ushort MajorVersion;
    public ushort MinorVersion;
    public short Ascender;
    public short Descender;
    public short LineGap;
    public ushort AdvanceWidthMax;
    public short MinLeftSideBearing;
    public short MinRightSideBearing;
    public short XMaxExtent;
    public short CaretSlopeRise;
    public short CaretSlopeRun;
    public short CaretOffset;
    public short Reserved1;
    public short Reserved2;
    public short Reserved3;
    public short Reserved4;
    public short MetricDataFormat;
    public ushort NumberOfHMetrics;

    public static HorizontalHeaderTable ReadFrom(Stream stream) => new()
    {
        MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Ascender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        Descender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        LineGap = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        AdvanceWidthMax = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinLeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        MinRightSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        XMaxExtent = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        CaretSlopeRise = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        CaretSlopeRun = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        CaretOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        Reserved1 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        Reserved2 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        Reserved3 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        Reserved4 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        MetricDataFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        NumberOfHMetrics = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
    };
}

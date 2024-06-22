using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class HorizontalHeaderTable
{
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required short Ascender { get; init; }
    public required short Descender { get; init; }
    public required short LineGap { get; init; }
    public required ushort AdvanceWidthMax { get; init; }
    public required short MinLeftSideBearing { get; init; }
    public required short MinRightSideBearing { get; init; }
    public required short XMaxExtent { get; init; }
    public required short CaretSlopeRise { get; init; }
    public required short CaretSlopeRun { get; init; }
    public required short CaretOffset { get; init; }
    public required short Reserved1 { get; init; }
    public required short Reserved2 { get; init; }
    public required short Reserved3 { get; init; }
    public required short Reserved4 { get; init; }
    public required short MetricDataFormat { get; init; }
    public required ushort NumberOfHMetrics { get; init; }

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

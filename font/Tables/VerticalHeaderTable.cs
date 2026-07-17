using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public record class VerticalHeaderTable : IExportable
{
    /* There are two versions of the 'vhea' table, 1.0 and 1.1. The difference between version 1.0 and version 1.1 is the name and definition of the following fields:
         ascender  becomes vertTypoAscender
         descender becomes vertTypoDescender
         lineGap   becomes vertTypoLineGap
     */
    public required Version16Dot16 Version { get; init; }
    public required FWORD Ascent { get; init; }
    public required FWORD Descent { get; init; }
    public required FWORD LineGap { get; init; }
    public required UFWORD AdvanceHeightMax { get; init; }
    public required FWORD MinTopSideBearing { get; init; }
    public required FWORD MinBottomSideBearing { get; init; }
    public required FWORD YMaxExtent { get; init; }
    public required short CaretSlopeRise { get; init; }
    public required short CaretSlopeRun { get; init; }
    public required short CaretOffset { get; init; }
    public short Reserved1 { get; init; } = 0;
    public short Reserved2 { get; init; } = 0;
    public short Reserved3 { get; init; } = 0;
    public short Reserved4 { get; init; } = 0;
    public short MetricDataFormat { get; init; } = 0;
    public required ushort NumberOfLongVerMetrics { get; init; }

    public static VerticalHeaderTable ReadFrom(Stream stream) => new()
    {
        Version = stream.ReadVersion16Dot16(),
        Ascent = stream.ReadFWORD(),
        Descent = stream.ReadFWORD(),
        LineGap = stream.ReadFWORD(),
        AdvanceHeightMax = stream.ReadUFWORD(),
        MinTopSideBearing = stream.ReadFWORD(),
        MinBottomSideBearing = stream.ReadFWORD(),
        YMaxExtent = stream.ReadFWORD(),
        CaretSlopeRise = stream.ReadShortByBigEndian(),
        CaretSlopeRun = stream.ReadShortByBigEndian(),
        CaretOffset = stream.ReadShortByBigEndian(),
        Reserved1 = stream.ReadShortByBigEndian(),
        Reserved2 = stream.ReadShortByBigEndian(),
        Reserved3 = stream.ReadShortByBigEndian(),
        Reserved4 = stream.ReadShortByBigEndian(),
        MetricDataFormat = stream.ReadShortByBigEndian(),
        NumberOfLongVerMetrics = stream.ReadUShortByBigEndian()
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteVersion16Dot16(Version);
        stream.WriteFWORD(Ascent);
        stream.WriteFWORD(Descent);
        stream.WriteFWORD(LineGap);
        stream.WriteUFWORD(AdvanceHeightMax);
        stream.WriteFWORD(MinTopSideBearing);
        stream.WriteFWORD(MinBottomSideBearing);
        stream.WriteFWORD(YMaxExtent);
        stream.WriteShortByBigEndian(CaretSlopeRise);
        stream.WriteShortByBigEndian(CaretSlopeRun);
        stream.WriteShortByBigEndian(CaretOffset);
        stream.WriteShortByBigEndian(Reserved1);
        stream.WriteShortByBigEndian(Reserved2);
        stream.WriteShortByBigEndian(Reserved3);
        stream.WriteShortByBigEndian(Reserved4);
        stream.WriteShortByBigEndian(MetricDataFormat);
        stream.WriteUShortByBigEndian(NumberOfLongVerMetrics);
    }
}

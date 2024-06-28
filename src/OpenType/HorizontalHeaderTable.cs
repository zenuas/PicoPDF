﻿using Mina.Extension;
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
        MajorVersion = stream.ReadUShortByBigEndian(),
        MinorVersion = stream.ReadUShortByBigEndian(),
        Ascender = stream.ReadShortByBigEndian(),
        Descender = stream.ReadShortByBigEndian(),
        LineGap = stream.ReadShortByBigEndian(),
        AdvanceWidthMax = stream.ReadUShortByBigEndian(),
        MinLeftSideBearing = stream.ReadShortByBigEndian(),
        MinRightSideBearing = stream.ReadShortByBigEndian(),
        XMaxExtent = stream.ReadShortByBigEndian(),
        CaretSlopeRise = stream.ReadShortByBigEndian(),
        CaretSlopeRun = stream.ReadShortByBigEndian(),
        CaretOffset = stream.ReadShortByBigEndian(),
        Reserved1 = stream.ReadShortByBigEndian(),
        Reserved2 = stream.ReadShortByBigEndian(),
        Reserved3 = stream.ReadShortByBigEndian(),
        Reserved4 = stream.ReadShortByBigEndian(),
        MetricDataFormat = stream.ReadShortByBigEndian(),
        NumberOfHMetrics = stream.ReadUShortByBigEndian(),
    };
}

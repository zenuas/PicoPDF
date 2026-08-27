using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class ValueRecord
{
    public required short? XPlacement { get; init; }
    public required short? YPlacement { get; init; }
    public required short? XAdvance { get; init; }
    public required short? YAdvance { get; init; }
    public required Offset16 XPlaDeviceOffset { get; init; }
    public required Offset16 YPlaDeviceOffset { get; init; }
    public required Offset16 XAdvDeviceOffset { get; init; }
    public required Offset16 YAdvDeviceOffset { get; init; }
    public required DeviceTable? XPlaDevice { get; init; }
    public required DeviceTable? YPlaDevice { get; init; }
    public required DeviceTable? XAdvDevice { get; init; }
    public required DeviceTable? YAdvDevice { get; init; }

    public static ValueRecord ReadFrom(Stream stream, ValueFormatFlags value_format)
    {
        var position = stream.Position;

        var xplacement = value_format.HasBit(ValueFormatFlags.X_PLACEMENT) ? (short?)stream.ReadShortByBigEndian() : null;
        var yplacement = value_format.HasBit(ValueFormatFlags.Y_PLACEMENT) ? (short?)stream.ReadShortByBigEndian() : null;
        var xadvance = value_format.HasBit(ValueFormatFlags.X_ADVANCE) ? (short?)stream.ReadShortByBigEndian() : null;
        var yadvance = value_format.HasBit(ValueFormatFlags.Y_ADVANCE) ? (short?)stream.ReadShortByBigEndian() : null;
        var xpladevice_offset = value_format.HasBit(ValueFormatFlags.X_PLACEMENT_DEVICE) ? stream.ReadOffset16() : 0;
        var ypladevice_offset = value_format.HasBit(ValueFormatFlags.Y_PLACEMENT_DEVICE) ? stream.ReadOffset16() : 0;
        var xadvdevice_offset = value_format.HasBit(ValueFormatFlags.X_ADVANCE_DEVICE) ? stream.ReadOffset16() : 0;
        var yadvdevice_offset = value_format.HasBit(ValueFormatFlags.Y_ADVANCE_DEVICE) ? stream.ReadOffset16() : 0;

        return new()
        {
            XPlacement = xplacement,
            YPlacement = yplacement,
            XAdvance = xadvance,
            YAdvance = yadvance,
            XPlaDeviceOffset = xpladevice_offset,
            YPlaDeviceOffset = ypladevice_offset,
            XAdvDeviceOffset = xadvdevice_offset,
            YAdvDeviceOffset = yadvdevice_offset,
            XPlaDevice = xpladevice_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + xpladevice_offset.Value)),
            YPlaDevice = ypladevice_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + ypladevice_offset.Value)),
            XAdvDevice = xadvdevice_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + xadvdevice_offset.Value)),
            YAdvDevice = yadvdevice_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + yadvdevice_offset.Value)),
        };
    }

    public override string ToString() => $"XPlacement: {XPlacement}, YPlacement: {YPlacement}, XAdvance: {XAdvance}, YAdvance: {YAdvance}";

    public int SizeOf() =>
        (XPlacement is { } ? sizeof(short) : 0) +
        (YPlacement is { } ? sizeof(short) : 0) +
        (XAdvance is { } ? sizeof(short) : 0) +
        (YAdvance is { } ? sizeof(short) : 0) +
        (XPlaDevice is { } ? Offset16.SizeOf() : 0) +
        (YPlaDevice is { } ? Offset16.SizeOf() : 0) +
        (XAdvDevice is { } ? Offset16.SizeOf() : 0) +
        (YAdvDevice is { } ? Offset16.SizeOf() : 0);

    public static int LoadSize(ValueFormatFlags value_format) =>
        (value_format.HasBit(ValueFormatFlags.X_PLACEMENT) ? sizeof(short) : 0) +
        (value_format.HasBit(ValueFormatFlags.Y_PLACEMENT) ? sizeof(short) : 0) +
        (value_format.HasBit(ValueFormatFlags.X_ADVANCE) ? sizeof(short) : 0) +
        (value_format.HasBit(ValueFormatFlags.Y_ADVANCE) ? sizeof(short) : 0) +
        (value_format.HasBit(ValueFormatFlags.X_PLACEMENT_DEVICE) ? Offset16.SizeOf() : 0) +
        (value_format.HasBit(ValueFormatFlags.Y_PLACEMENT_DEVICE) ? Offset16.SizeOf() : 0) +
        (value_format.HasBit(ValueFormatFlags.X_ADVANCE_DEVICE) ? Offset16.SizeOf() : 0) +
        (value_format.HasBit(ValueFormatFlags.Y_ADVANCE_DEVICE) ? Offset16.SizeOf() : 0);
}

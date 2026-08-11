using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class ValueRecord
{
    public required short XPlacement { get; init; }
    public required short YPlacement { get; init; }
    public required short XAdvance { get; init; }
    public required short YAdvance { get; init; }
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

        var xplacement = value_format.HasBit(ValueFormatFlags.X_PLACEMENT) ? stream.ReadShortByBigEndian() : (short)0;
        var yplacement = value_format.HasBit(ValueFormatFlags.Y_PLACEMENT) ? stream.ReadShortByBigEndian() : (short)0;
        var xadvance = value_format.HasBit(ValueFormatFlags.X_ADVANCE) ? stream.ReadShortByBigEndian() : (short)0;
        var yadvance = value_format.HasBit(ValueFormatFlags.Y_ADVANCE) ? stream.ReadShortByBigEndian() : (short)0;
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
}

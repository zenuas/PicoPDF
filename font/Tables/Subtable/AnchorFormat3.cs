using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class AnchorFormat3 : ISubtable, IAnchorFormat
{
    public required ushort Format { get; init; }
    public required short XCoordinate { get; init; }
    public required short YCoordinate { get; init; }
    public required Offset16 XDeviceOffset { get; init; }
    public required Offset16 YDeviceOffset { get; init; }
    public required DeviceTable? XDevice { get; init; }
    public required DeviceTable? YDevice { get; init; }

    public static AnchorFormat3 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var x_cordinate = stream.ReadShortByBigEndian();
        var y_cordinate = stream.ReadShortByBigEndian();
        var x_device_offset = stream.ReadOffset16();
        var y_device_offset = stream.ReadOffset16();

        return new()
        {
            Format = 3,
            XCoordinate = x_cordinate,
            YCoordinate = y_cordinate,
            XDeviceOffset = x_device_offset,
            YDeviceOffset = y_device_offset,
            XDevice = x_device_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + x_device_offset.Value)),
            YDevice = y_device_offset.Value == 0 ? null : DeviceTable.ReadFrom(stream.SeekTo(position + y_device_offset.Value)),
        };
    }
}

using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class DeviceTable
{
    public required ushort StartSize { get; init; }
    public required ushort EndSize { get; init; }
    public required ushort DeltaFormat { get; init; }
    public required ushort[] DeltaValue { get; init; }

    public static DeviceTable ReadFrom(Stream stream)
    {
        var start_size = stream.ReadUShortByBigEndian();
        var end_size = stream.ReadUShortByBigEndian();
        var delta_format = stream.ReadUShortByBigEndian();

        return new()
        {
            StartSize = start_size,
            EndSize = end_size,
            DeltaFormat = delta_format,
            DeltaValue = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(end_size - start_size + 1)],
        };
    }
}

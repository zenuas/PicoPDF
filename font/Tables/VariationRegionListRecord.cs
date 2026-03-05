using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class VariationRegionListRecord : IExportable
{
    public required ushort AxisCount { get; init; }
    public required ushort RegionCount { get; init; }
    public required RegionAxisCoordinatesRecord[] VariationRegions { get; init; }

    public static VariationRegionListRecord ReadFrom(Stream stream)
    {
        var axisCount = stream.ReadUShortByBigEndian();
        var regionCount = stream.ReadUShortByBigEndian();
        if (regionCount >= 32768) throw new();

        return new()
        {
            AxisCount = axisCount,
            RegionCount = regionCount,
            VariationRegions = [.. Lists.Repeat(() => RegionAxisCoordinatesRecord.ReadFrom(stream, axisCount)).Take(regionCount)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(AxisCount);
        stream.WriteUShortByBigEndian(RegionCount);
        VariationRegions.Each(x => x.WriteTo(stream));
    }

    public int SizeOf() => AxisCount.SizeOf() + RegionCount.SizeOf() + VariationRegions.Select(x => x.SizeOf()).Sum();
}

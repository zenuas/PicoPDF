using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class RegionAxisCoordinatesRecord : IExportable
{
    public required ushort[] RegionAxes { get; init; }

    public static RegionAxisCoordinatesRecord ReadFrom(Stream stream, int axisCount) => new()
    {
        RegionAxes = [.. Lists.Repeat(() => stream.ReadUShortByBigEndian()).Take(axisCount)],
    };

    public void WriteTo(Stream stream)
    {
        RegionAxes.Each(stream.WriteUShortByBigEndian);
    }

    public int SizeOf() => sizeof(ushort) * RegionAxes.Length;
}

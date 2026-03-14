using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class ColorLine : IExportable, IColorLine
{
    public required byte Extend { get; init; }
    public required ushort NumberOfStops { get; init; }
    public required ColorStop[] ColorStops { get; init; }

    public static ColorLine ReadFrom(Stream stream, long position, Dictionary<long, IColorLine> colorLineCache) => (ColorLine)(colorLineCache.TryGetValue(position, out var line) ? line : colorLineCache[position] = ReadFrom(stream.SeekTo(position)));

    public static ColorLine ReadFrom(Stream stream)
    {
        var extend = stream.ReadUByte();
        var numStops = stream.ReadUShortByBigEndian();
        return new()
        {
            Extend = extend,
            NumberOfStops = numStops,
            ColorStops = [.. Lists.Repeat(() => ColorStop.ReadFrom(stream)).Take(numStops)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Extend);
        stream.WriteUShortByBigEndian((ushort)ColorStops.Length);
        ColorStops.Each(x => x.WriteTo(stream));
    }

    public int SizeOf() => Extend.SizeOf() + NumberOfStops.SizeOf() + ColorStops.Select(x => x.SizeOf()).Sum();
}

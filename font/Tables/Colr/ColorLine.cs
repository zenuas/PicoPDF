using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class ColorLine : IExportable, IColorLine
{
    public required Extends Extend { get; init; }
    public required ushort NumberOfStops { get; init; }
    public required ColorStop[] ColorStops { get; init; }

    public static ColorLine ReadFrom(Stream stream, long position, Dictionary<long, IColorLine> colorLineCache) => (ColorLine)(colorLineCache.TryGetValue(position, out var line) ? line : colorLineCache[position] = ReadFrom(stream.SeekTo(position)));

    public static ColorLine ReadFrom(Stream stream)
    {
        var extend = stream.ReadUByte();
        var numStops = stream.ReadUShortByBigEndian();
        return new()
        {
            Extend = (Extends)extend,

            // A color line requires at least one color stop to paint any color values.
            // If the color line does not have any color stops, then transparent black is used for the entire color line.
            // If only one color stop is specified, that color is used for the entire color line.
            // At least two color stops are needed to create color gradation.
            NumberOfStops = numStops,
            ColorStops = [.. Lists.Repeat(() => ColorStop.ReadFrom(stream)).Take(numStops)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte((byte)Extend);
        stream.WriteUShortByBigEndian((ushort)ColorStops.Length);
        ColorStops.Each(x => x.WriteTo(stream));
    }

    public int SizeOf() => Extend.SizeOf() + NumberOfStops.SizeOf() + ColorStops.Select(x => x.SizeOf()).Sum();

    public override string ToString() => $"Extend={Extend}, ColorStops=[({string.Join("), (", ColorStops)})]";
}

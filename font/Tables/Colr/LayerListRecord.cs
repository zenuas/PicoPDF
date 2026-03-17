using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class LayerListRecord : IExportable
{
    public required uint NumberOfLayers { get; init; }
    public required uint[] PaintOffsets { get; init; }
    public required IPaintFormat[] Paints { get; init; }

    public static LayerListRecord ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position;

        var numLayers = stream.ReadUIntByBigEndian();
        var paintOffsets = Lists.Repeat(stream.ReadOffset32).Take((int)numLayers).ToArray();

        return new()
        {
            NumberOfLayers = numLayers,
            PaintOffsets = paintOffsets,
            Paints = [.. paintOffsets.Select(x => PaintFormat.ReadFrom(stream, position + x, paintCache, colorLineCache, affineCache))],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian((uint)Paints.Length);

        using var mem = new MemoryStream();
        foreach (var paint in Paints)
        {
            stream.WriteOffset32((uint)(SizeOf() + mem.Length));
            paint.WriteTo(mem);
        }
        stream.Write(mem.ToArray());
    }

    public int SizeOf() => NumberOfLayers.SizeOf() + (sizeof(uint) * Paints.Length);
}

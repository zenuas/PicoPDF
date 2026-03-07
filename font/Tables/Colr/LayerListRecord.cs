using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class LayerListRecord : IExportable
{
    public required uint NumberLayers { get; init; }
    public required uint[] PaintOffsets { get; init; }
    public required IPaintFormat[] Paints { get; init; }

    public static LayerListRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var numLayers = stream.ReadUIntByBigEndian();
        var paintOffsets = Lists.Repeat(stream.ReadUIntByBigEndian).Take((int)numLayers).ToArray();

        return new()
        {
            NumberLayers = numLayers,
            PaintOffsets = paintOffsets,
            Paints = [.. paintOffsets.Select(x => PaintFormat.ReadFrom(stream.SeekTo(position + x)))],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian((uint)Paints.Length);

        using var mem = new MemoryStream();
        foreach (var paint in Paints)
        {
            stream.WriteUIntByBigEndian((uint)(SizeOf() + mem.Length));
            paint.WriteTo(mem);
        }
        stream.Write(mem.ToArray());
    }

    public int SizeOf() => NumberLayers.SizeOf() + (sizeof(uint) * Paints.Length);
}

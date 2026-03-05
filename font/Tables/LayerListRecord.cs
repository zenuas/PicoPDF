using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class LayerListRecord : IExportable
{
    public required uint NumberLayers { get; init; }
    public required uint[] PaintOffsets { get; init; }

    public static LayerListRecord ReadFrom(Stream stream)
    {
        var numLayers = stream.ReadUIntByBigEndian();

        return new()
        {
            NumberLayers = numLayers,
            PaintOffsets = [.. Lists.Repeat(stream.ReadUIntByBigEndian).Take((int)numLayers)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian((uint)PaintOffsets.Length);
        PaintOffsets.Each(stream.WriteUIntByBigEndian);
    }

    public int SizeOf() => NumberLayers.SizeOf() + (sizeof(uint) * PaintOffsets.Length);
}

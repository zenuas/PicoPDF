using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class LayerListRecord
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
        stream.WriteUIntByBigEndian(NumberLayers);
        PaintOffsets.Each(stream.WriteUIntByBigEndian);
    }
}

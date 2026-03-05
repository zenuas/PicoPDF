using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrLayersFormat1 : IPaintColrLayersFormat
{
    public required byte Format { get; init; }
    public required byte NumberOfLayers { get; init; }
    public required uint FirstLayerIndex { get; init; }

    public static PaintColrLayersFormat1 ReadFrom(Stream stream) => new()
    {
        Format = 1,
        NumberOfLayers = stream.ReadUByte(),
        FirstLayerIndex = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteByte(NumberOfLayers);
        stream.WriteUIntByBigEndian(FirstLayerIndex);
    }

    public int SizeOf() => Format.SizeOf() + NumberOfLayers.SizeOf() + FirstLayerIndex.SizeOf();
}

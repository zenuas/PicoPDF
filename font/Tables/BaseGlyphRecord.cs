using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class BaseGlyphRecord
{
    public required ushort GlyphID { get; init; }
    public required ushort FirstLayerIndex { get; init; }
    public required ushort NumberLayers { get; init; }

    public static BaseGlyphRecord ReadFrom(Stream stream) => new()
    {
        GlyphID = stream.ReadUShortByBigEndian(),
        FirstLayerIndex = stream.ReadUShortByBigEndian(),
        NumberLayers = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(GlyphID);
        stream.WriteUShortByBigEndian(FirstLayerIndex);
        stream.WriteUShortByBigEndian(NumberLayers);
    }
}

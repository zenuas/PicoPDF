using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class BaseGlyphRecord : IExportable
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

    public int SizeOf() => GlyphID.SizeOf() + FirstLayerIndex.SizeOf() + NumberLayers.SizeOf();
}

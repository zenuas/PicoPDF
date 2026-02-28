using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class LayerRecord
{
    public required ushort GlyphID { get; init; }
    public required ushort PaletteIndex { get; init; }

    public static LayerRecord ReadFrom(Stream stream) => new()
    {
        GlyphID = stream.ReadUShortByBigEndian(),
        PaletteIndex = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(GlyphID);
        stream.WriteUShortByBigEndian(PaletteIndex);
    }
}

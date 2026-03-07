using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class LayerRecord : IExportable
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

    public int SizeOf() => GlyphID.SizeOf() + PaletteIndex.SizeOf();
}

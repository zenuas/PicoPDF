using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrLayersFormat2 : IPaintColrLayersFormat
{
    public required byte Format { get; init; }
    public required ushort PaletteIndex { get; init; }
    public required ushort Alpha { get; init; }

    public static PaintColrLayersFormat2 ReadFrom(Stream stream) => new()
    {
        Format = 2,
        PaletteIndex = stream.ReadUShortByBigEndian(),
        Alpha = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteUShortByBigEndian(PaletteIndex);
        stream.WriteUShortByBigEndian(Alpha);
    }

    public int SizeOf() => Format.SizeOf() + PaletteIndex.SizeOf() + Alpha.SizeOf();
}

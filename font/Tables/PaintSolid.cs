using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintSolid : IPaintFormat
{
    public required byte Format { get; init; }
    public required ushort PaletteIndex { get; init; }
    public required ushort Alpha { get; init; }

    public static PaintSolid ReadFrom(Stream stream) => new()
    {
        Format = 2,
        PaletteIndex = stream.ReadUShortByBigEndian(),
        Alpha = stream.ReadF2DOT14(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteUShortByBigEndian(PaletteIndex);
        stream.WriteF2DOT14(Alpha);
    }

    public int SizeOf() => Format.SizeOf() + PaletteIndex.SizeOf() + Alpha.SizeOf();
}

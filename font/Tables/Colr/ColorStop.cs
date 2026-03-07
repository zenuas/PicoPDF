using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class ColorStop : IExportable
{
    public required ushort StopOffset { get; init; }
    public required ushort PaletteIndex { get; init; }
    public required ushort Alpha { get; init; }

    public static ColorStop ReadFrom(Stream stream) => new()
    {
        StopOffset = stream.ReadF2DOT14(),
        PaletteIndex = stream.ReadUShortByBigEndian(),
        Alpha = stream.ReadF2DOT14(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteF2DOT14(StopOffset);
        stream.WriteUShortByBigEndian(PaletteIndex);
        stream.WriteF2DOT14(Alpha);
    }

    public int SizeOf() => StopOffset.SizeOf() + StopOffset.SizeOf() + Alpha.SizeOf();
}

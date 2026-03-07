using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class VarColorStop : IExportable
{
    public required ushort StopOffset { get; init; }
    public required ushort PaletteIndex { get; init; }
    public required ushort Alpha { get; init; }
    public required uint VarIndexBase { get; init; }

    public static VarColorStop ReadFrom(Stream stream) => new()
    {
        StopOffset = stream.ReadF2DOT14(),
        PaletteIndex = stream.ReadUShortByBigEndian(),
        Alpha = stream.ReadF2DOT14(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteF2DOT14(StopOffset);
        stream.WriteUShortByBigEndian(PaletteIndex);
        stream.WriteF2DOT14(Alpha);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => StopOffset.SizeOf() + StopOffset.SizeOf() + Alpha.SizeOf() + VarIndexBase.SizeOf();
}

using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarSolid : IPaintFormat
{
    public required byte Format { get; init; }
    public required ushort PaletteIndex { get; init; }
    public required ushort Alpha { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarSolid ReadFrom(Stream stream) => new()
    {
        Format = 3,
        PaletteIndex = stream.ReadUShortByBigEndian(),
        Alpha = stream.ReadUShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteUShortByBigEndian(PaletteIndex);
        stream.WriteUShortByBigEndian(Alpha);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + PaletteIndex.SizeOf() + Alpha.SizeOf() + VarIndexBase.SizeOf();
}

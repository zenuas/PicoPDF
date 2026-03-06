using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarScale : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarScale ReadFrom(Stream stream) => new()
    {
        Format = 17,
        PaintOffset = stream.ReadOffset24(),
        ScaleX = stream.ReadF2DOT14(),
        ScaleY = stream.ReadF2DOT14(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(ScaleX);
        stream.WriteF2DOT14(ScaleY);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        VarIndexBase.SizeOf();
}

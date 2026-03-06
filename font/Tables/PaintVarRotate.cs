using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarRotate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Angle { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarRotate ReadFrom(Stream stream) => new()
    {
        Format = 25,
        PaintOffset = stream.ReadOffset24(),
        Angle = stream.ReadF2DOT14(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(Angle);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Angle.SizeOf() +
        VarIndexBase.SizeOf();
}

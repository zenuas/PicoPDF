using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarSkew : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort XSkewAngle { get; init; }
    public required ushort YSkewAngle { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarSkew ReadFrom(Stream stream) => new()
    {
        Format = 29,
        PaintOffset = stream.ReadOffset24(),
        XSkewAngle = stream.ReadF2DOT14(),
        YSkewAngle = stream.ReadF2DOT14(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(XSkewAngle);
        stream.WriteF2DOT14(YSkewAngle);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        XSkewAngle.SizeOf() + YSkewAngle.SizeOf() +
        VarIndexBase.SizeOf();
}

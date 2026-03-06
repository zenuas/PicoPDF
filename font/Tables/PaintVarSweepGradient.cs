using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarSweepGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required ushort StartAngle { get; init; }
    public required ushort EndAngle { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarSweepGradient ReadFrom(Stream stream)
    {
        var colorLineOffset = stream.ReadOffset24();
        return new()
        {
            Format = 9,
            ColorLineOffset = colorLineOffset,
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
            StartAngle = stream.ReadF2DOT14(),
            EndAngle = stream.ReadF2DOT14(),
            VarIndexBase = stream.ReadUIntByBigEndian(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(ColorLineOffset);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
        stream.WriteF2DOT14(StartAngle);
        stream.WriteF2DOT14(EndAngle);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset.SizeOf() */Const.SizeofOffset24 +
        CenterX.SizeOf() + CenterY.SizeOf() +
        StartAngle.SizeOf() + EndAngle.SizeOf() +
        VarIndexBase.SizeOf();
}

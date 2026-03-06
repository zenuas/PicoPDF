using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintSkewAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort XSkewAngle { get; init; }
    public required ushort YSkewAngle { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }

    public static PaintSkewAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 30,
        PaintOffset = stream.Read3BytesByBigEndian(),
        XSkewAngle = stream.ReadF2DOT14(),
        YSkewAngle = stream.ReadF2DOT14(),
        CenterX = stream.ReadFWORD(),
        CenterY = stream.ReadFWORD(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteF2DOT14(XSkewAngle);
        stream.WriteF2DOT14(YSkewAngle);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        XSkewAngle.SizeOf() + YSkewAngle.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

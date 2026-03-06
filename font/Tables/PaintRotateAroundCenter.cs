using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintRotateAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Angle { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }

    public static PaintRotateAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 26,
        PaintOffset = stream.Read3BytesByBigEndian(),
        Angle = stream.ReadF2DOT14(),
        CenterX = stream.ReadFWORD(),
        CenterY = stream.ReadFWORD(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteF2DOT14(Angle);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Angle.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

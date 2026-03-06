using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintSkew : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort XSkewAngle { get; init; }
    public required ushort YSkewAngle { get; init; }

    public static PaintSkew ReadFrom(Stream stream) => new()
    {
        Format = 28,
        PaintOffset = stream.ReadOffset24(),
        XSkewAngle = stream.ReadF2DOT14(),
        YSkewAngle = stream.ReadF2DOT14(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(XSkewAngle);
        stream.WriteF2DOT14(YSkewAngle);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        XSkewAngle.SizeOf() + YSkewAngle.SizeOf();
}

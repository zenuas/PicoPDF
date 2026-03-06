using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintRotate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Angle { get; init; }

    public static PaintRotate ReadFrom(Stream stream) => new()
    {
        Format = 24,
        PaintOffset = stream.ReadOffset24(),
        Angle = stream.ReadF2DOT14(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(Angle);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Angle.SizeOf();
}

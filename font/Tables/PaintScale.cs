using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScale : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }

    public static PaintScale ReadFrom(Stream stream)
    {
        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 16,
            PaintOffset = paintOffset,
            ScaleX = stream.ReadF2DOT14(),
            ScaleY = stream.ReadF2DOT14(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(ScaleX);
        stream.WriteF2DOT14(ScaleY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf();
}

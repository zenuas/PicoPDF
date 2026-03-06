using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScaleUniform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }

    public static PaintScaleUniform ReadFrom(Stream stream)
    {
        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 20,
            PaintOffset = paintOffset,
            Scale = stream.ReadF2DOT14(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(Scale);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Scale.SizeOf();
}

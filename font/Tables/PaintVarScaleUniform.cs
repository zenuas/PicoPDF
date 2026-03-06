using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarScaleUniform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarScaleUniform ReadFrom(Stream stream)
    {
        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 21,
            PaintOffset = paintOffset,
            Scale = stream.ReadF2DOT14(),
            VarIndexBase = stream.ReadUIntByBigEndian(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(Scale);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Scale.SizeOf() +
        VarIndexBase.SizeOf();
}

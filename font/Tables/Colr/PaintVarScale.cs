using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintVarScale : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }
    public required uint VarIndexBase { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintVarScale ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 17,
            PaintOffset = paintOffset,
            ScaleX = stream.ReadF2DOT14(),
            ScaleY = stream.ReadF2DOT14(),
            VarIndexBase = stream.ReadUIntByBigEndian(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteF2DOT14(ScaleX);
        stream.WriteF2DOT14(ScaleY);
        stream.WriteUIntByBigEndian(VarIndexBase);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        VarIndexBase.SizeOf();
}

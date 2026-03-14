using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintScaleUniform : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintScaleUniform ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 20,
            PaintOffset = paintOffset,
            Scale = stream.ReadF2DOT14(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteF2DOT14(Scale);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        Scale.SizeOf();
}

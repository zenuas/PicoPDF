using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintScaleUniformAroundCenter : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintScaleUniformAroundCenter ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 22,
            PaintOffset = paintOffset,
            Scale = stream.ReadF2DOT14(),
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteF2DOT14(Scale);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        Scale.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

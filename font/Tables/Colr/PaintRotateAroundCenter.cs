using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintRotateAroundCenter : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Angle { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintRotateAroundCenter ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 26,
            PaintOffset = paintOffset,
            Angle = stream.ReadF2DOT14(),
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteF2DOT14(Angle);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        Angle.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

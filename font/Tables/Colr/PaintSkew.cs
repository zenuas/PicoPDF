using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintSkew : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required F2DOT14 XSkewAngle { get; init; }
    public required F2DOT14 YSkewAngle { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintSkew ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 28,
            PaintOffset = paintOffset,
            XSkewAngle = stream.ReadF2DOT14(),
            YSkewAngle = stream.ReadF2DOT14(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteF2DOT14(XSkewAngle);
        stream.WriteF2DOT14(YSkewAngle);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        XSkewAngle.SizeOf() + YSkewAngle.SizeOf();
}

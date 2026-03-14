using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintTranslate : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required short DX { get; init; }
    public required short DY { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintTranslate ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 14,
            PaintOffset = paintOffset,
            DX = stream.ReadFWORD(),
            DY = stream.ReadFWORD(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteFWORD(DX);
        stream.WriteFWORD(DY);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        DX.SizeOf() + DY.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintVarTransform : IPaintFormat, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required int TransformOffset { get; init; }
    public required IPaintFormat Paint { get; init; }
    public required VarAffine2x3 Transform { get; init; }

    public static PaintVarTransform ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        var transformOffset = stream.ReadOffset24();
        return new()
        {
            Format = 13,
            PaintOffset = paintOffset,
            TransformOffset = transformOffset,
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache, colorLineCache, affineCache),
            Transform = VarAffine2x3.ReadFrom(stream, position + transformOffset, affineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf() + Transform.SizeOf());
        stream.WriteOffset24(SizeOf());
        Transform.WriteTo(stream);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 + /* TransformOffset */Const.SizeofOffset24;
}

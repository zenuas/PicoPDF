using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintScaleUniformAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintScaleUniformAroundCenter ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 22,
            PaintOffset = paintOffset,
            Scale = stream.ReadF2DOT14(),
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
            Paint = PaintFormat.ReadFrom(stream.SeekTo(position + paintOffset)),
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

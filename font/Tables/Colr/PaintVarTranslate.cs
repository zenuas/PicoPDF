using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintVarTranslate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required short DX { get; init; }
    public required short DY { get; init; }
    public required uint VarIndexBase { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintVarTranslate ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 15,
            PaintOffset = paintOffset,
            DX = stream.ReadFWORD(),
            DY = stream.ReadFWORD(),
            VarIndexBase = stream.ReadUIntByBigEndian(),
            Paint = PaintFormat.ReadFrom(stream.SeekTo(position + paintOffset)),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteFWORD(DX);
        stream.WriteFWORD(DY);
        stream.WriteUIntByBigEndian(VarIndexBase);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 +
        DX.SizeOf() + DY.SizeOf() +
        VarIndexBase.SizeOf();
}

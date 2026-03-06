using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScaleAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }

    public static PaintScaleAroundCenter ReadFrom(Stream stream)
    {
        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 18,
            PaintOffset = paintOffset,
            ScaleX = stream.ReadF2DOT14(),
            ScaleY = stream.ReadF2DOT14(),
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteF2DOT14(ScaleX);
        stream.WriteF2DOT14(ScaleY);
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintTranslate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required short DX { get; init; }
    public required short DY { get; init; }

    public static PaintTranslate ReadFrom(Stream stream) => new()
    {
        Format = 14,
        PaintOffset = stream.ReadOffset24(),
        DX = stream.ReadFWORD(),
        DY = stream.ReadFWORD(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteFWORD(DX);
        stream.WriteFWORD(DY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        DX.SizeOf() + DY.SizeOf();
}

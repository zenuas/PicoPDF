using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintComposite : IPaintFormat
{
    public required byte Format { get; init; }
    public required int SourcePaintOffset { get; init; }
    public required byte CompositeMode { get; init; }
    public required int BackdropPaintOffset { get; init; }

    public static PaintComposite ReadFrom(Stream stream) => new()
    {
        Format = 32,
        SourcePaintOffset = stream.Read3BytesByBigEndian(),
        CompositeMode = stream.ReadUByte(),
        BackdropPaintOffset = stream.Read3BytesByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(SourcePaintOffset);
        stream.WriteByte(CompositeMode);
        stream.Write3BytesByBigEndian(BackdropPaintOffset);
    }

    public int SizeOf() => Format.SizeOf() +
        /* SourcePaintOffset.SizeOf() */Const.SizeofOffset24 +
        CompositeMode.SizeOf() +
        /* BackdropPaintOffset.SizeOf() */Const.SizeofOffset24;
}

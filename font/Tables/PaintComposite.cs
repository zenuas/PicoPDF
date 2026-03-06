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

    public static PaintComposite ReadFrom(Stream stream)
    {
        var sourcePaintOffset = stream.ReadOffset24();
        var backdropPaintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 32,
            SourcePaintOffset = sourcePaintOffset,
            CompositeMode = stream.ReadUByte(),
            BackdropPaintOffset = backdropPaintOffset,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SourcePaintOffset);
        stream.WriteByte(CompositeMode);
        stream.WriteOffset24(BackdropPaintOffset);
    }

    public int SizeOf() => Format.SizeOf() +
        /* SourcePaintOffset.SizeOf() */Const.SizeofOffset24 +
        CompositeMode.SizeOf() +
        /* BackdropPaintOffset.SizeOf() */Const.SizeofOffset24;
}

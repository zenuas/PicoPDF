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
    public required IPaintFormat SourcePaint { get; init; }
    public required IPaintFormat BackdropPaint { get; init; }

    public static PaintComposite ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var sourcePaintOffset = stream.ReadOffset24();
        var compositeMode = stream.ReadUByte();
        var backdropPaintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 32,
            SourcePaintOffset = sourcePaintOffset,
            CompositeMode = compositeMode,
            BackdropPaintOffset = backdropPaintOffset,
            SourcePaint = PaintFormat.ReadFrom(stream.SeekTo(position + sourcePaintOffset)),
            BackdropPaint = PaintFormat.ReadFrom(stream.SeekTo(position + backdropPaintOffset)),
        };
    }

    public void WriteTo(Stream stream)
    {
        using var mem = new MemoryStream();
        SourcePaint.WriteTo(mem);

        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteByte(CompositeMode);
        stream.WriteOffset24(SizeOf() + (int)mem.Length);
        stream.Write(mem.ToArray());
        BackdropPaint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() +
        /* SourcePaintOffset */Const.SizeofOffset24 +
        CompositeMode.SizeOf() +
        /* BackdropPaintOffset */Const.SizeofOffset24;
}

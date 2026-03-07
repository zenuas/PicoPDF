using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintTransform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required int TransformOffset { get; init; }
    public required IPaintFormat Paint { get; init; }
    public required Affine2x3 Transform { get; init; }

    public static PaintTransform ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var paintOffset = stream.ReadOffset24();
        var transformOffset = stream.ReadOffset24();
        return new()
        {
            Format = 12,
            PaintOffset = paintOffset,
            TransformOffset = transformOffset,
            Paint = PaintFormat.ReadFrom(stream.SeekTo(position + paintOffset)),
            Transform = Affine2x3.ReadFrom(stream.SeekTo(position + transformOffset)),
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

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 + /* TransformOffset.SizeOf() */Const.SizeofOffset24;
}

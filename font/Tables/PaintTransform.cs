using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintTransform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required int TransformOffset { get; init; }

    public static PaintTransform ReadFrom(Stream stream) => new()
    {
        Format = 12,
        PaintOffset = stream.ReadOffset24(),
        TransformOffset = stream.ReadOffset24(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(PaintOffset);
        stream.WriteOffset24(TransformOffset);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 + /* TransformOffset.SizeOf() */Const.SizeofOffset24;
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarTransform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required int TransformOffset { get; init; }

    public static PaintVarTransform ReadFrom(Stream stream) => new()
    {
        Format = 13,
        PaintOffset = stream.Read3BytesByBigEndian(),
        TransformOffset = stream.Read3BytesByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.Write3BytesByBigEndian(TransformOffset);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 + /* TransformOffset.SizeOf() */Const.SizeofOffset24;
}

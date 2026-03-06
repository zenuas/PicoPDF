using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScale : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }

    public static PaintScale ReadFrom(Stream stream) => new()
    {
        Format = 16,
        PaintOffset = stream.Read3BytesByBigEndian(),
        ScaleX = stream.ReadUShortByBigEndian(),
        ScaleY = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(ScaleX);
        stream.WriteUShortByBigEndian(ScaleY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf();
}

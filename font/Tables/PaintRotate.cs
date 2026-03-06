using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintRotate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Angle { get; init; }

    public static PaintRotate ReadFrom(Stream stream) => new()
    {
        Format = 24,
        PaintOffset = stream.Read3BytesByBigEndian(),
        Angle = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(Angle);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        Angle.SizeOf();
}

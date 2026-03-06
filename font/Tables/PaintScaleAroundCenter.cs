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

    public static PaintScaleAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 18,
        PaintOffset = stream.Read3BytesByBigEndian(),
        ScaleX = stream.ReadUShortByBigEndian(),
        ScaleY = stream.ReadUShortByBigEndian(),
        CenterX = stream.ReadShortByBigEndian(),
        CenterY = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(ScaleX);
        stream.WriteUShortByBigEndian(ScaleY);
        stream.WriteShortByBigEndian(CenterX);
        stream.WriteShortByBigEndian(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

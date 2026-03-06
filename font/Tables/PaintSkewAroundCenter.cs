using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintSkewAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort XSkewAngle { get; init; }
    public required ushort YSkewAngle { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }

    public static PaintSkewAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 30,
        PaintOffset = stream.Read3BytesByBigEndian(),
        XSkewAngle = stream.ReadUShortByBigEndian(),
        YSkewAngle = stream.ReadUShortByBigEndian(),
        CenterX = stream.ReadShortByBigEndian(),
        CenterY = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(XSkewAngle);
        stream.WriteUShortByBigEndian(YSkewAngle);
        stream.WriteShortByBigEndian(CenterX);
        stream.WriteShortByBigEndian(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        XSkewAngle.SizeOf() + YSkewAngle.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

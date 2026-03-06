using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintSweepGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required ushort StartAngle { get; init; }
    public required ushort EndAngle { get; init; }

    public static PaintSweepGradient ReadFrom(Stream stream) => new()
    {
        Format = 8,
        ColorLineOffset = stream.Read3BytesByBigEndian(),
        CenterX = stream.ReadShortByBigEndian(),
        CenterY = stream.ReadShortByBigEndian(),
        StartAngle = stream.ReadUShortByBigEndian(),
        EndAngle = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(ColorLineOffset);
        stream.WriteShortByBigEndian(CenterX);
        stream.WriteShortByBigEndian(CenterY);
        stream.WriteUShortByBigEndian(StartAngle);
        stream.WriteUShortByBigEndian(EndAngle);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset sizeof(Offset24) */3 +
        CenterX.SizeOf() + CenterY.SizeOf() +
        StartAngle.SizeOf() + EndAngle.SizeOf();
}

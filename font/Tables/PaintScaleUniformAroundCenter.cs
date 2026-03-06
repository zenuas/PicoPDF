using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScaleUniformAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }

    public static PaintScaleUniformAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 22,
        PaintOffset = stream.Read3BytesByBigEndian(),
        Scale = stream.ReadUShortByBigEndian(),
        CenterX = stream.ReadShortByBigEndian(),
        CenterY = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(Scale);
        stream.WriteShortByBigEndian(CenterX);
        stream.WriteShortByBigEndian(CenterY);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset sizeof(Offset24) */3 +
        Scale.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf();
}

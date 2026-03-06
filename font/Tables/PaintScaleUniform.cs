using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintScaleUniform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }

    public static PaintScaleUniform ReadFrom(Stream stream) => new()
    {
        Format = 20,
        PaintOffset = stream.Read3BytesByBigEndian(),
        Scale = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(Scale);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset sizeof(Offset24) */3 +
        Scale.SizeOf();
}

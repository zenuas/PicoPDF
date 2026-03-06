using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarScaleUniform : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort Scale { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarScaleUniform ReadFrom(Stream stream) => new()
    {
        Format = 21,
        PaintOffset = stream.Read3BytesByBigEndian(),
        Scale = stream.ReadUShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(Scale);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset sizeof(Offset24) */3 +
        Scale.SizeOf() +
        VarIndexBase.SizeOf();
}

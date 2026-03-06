using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarScale : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarScale ReadFrom(Stream stream) => new()
    {
        Format = 17,
        PaintOffset = stream.Read3BytesByBigEndian(),
        ScaleX = stream.ReadUShortByBigEndian(),
        ScaleY = stream.ReadUShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(ScaleX);
        stream.WriteUShortByBigEndian(ScaleY);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset sizeof(Offset24) */3 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        VarIndexBase.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarTranslate : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required short DX { get; init; }
    public required short DY { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarTranslate ReadFrom(Stream stream) => new()
    {
        Format = 15,
        PaintOffset = stream.Read3BytesByBigEndian(),
        DX = stream.ReadShortByBigEndian(),
        DY = stream.ReadShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteShortByBigEndian(DX);
        stream.WriteShortByBigEndian(DY);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset sizeof(Offset24) */3 +
        DX.SizeOf() + DY.SizeOf() +
        VarIndexBase.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintVarScaleAroundCenter : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort ScaleX { get; init; }
    public required ushort ScaleY { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintVarScaleAroundCenter ReadFrom(Stream stream) => new()
    {
        Format = 19,
        PaintOffset = stream.Read3BytesByBigEndian(),
        ScaleX = stream.ReadUShortByBigEndian(),
        ScaleY = stream.ReadUShortByBigEndian(),
        CenterX = stream.ReadShortByBigEndian(),
        CenterY = stream.ReadShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(ScaleX);
        stream.WriteUShortByBigEndian(ScaleY);
        stream.WriteShortByBigEndian(CenterX);
        stream.WriteShortByBigEndian(CenterY);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 +
        ScaleX.SizeOf() + ScaleY.SizeOf() +
        CenterX.SizeOf() + CenterY.SizeOf() +
        VarIndexBase.SizeOf();
}

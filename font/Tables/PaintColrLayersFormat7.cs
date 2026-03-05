using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrLayersFormat7 : IPaintColrLayersFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short X0 { get; init; }
    public required short Y0 { get; init; }
    public required ushort Radius0 { get; init; }
    public required short X1 { get; init; }
    public required short Y1 { get; init; }
    public required ushort Radius1 { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintColrLayersFormat7 ReadFrom(Stream stream) => new()
    {
        Format = 7,
        ColorLineOffset = stream.Read3BytesByBigEndian(),
        X0 = stream.ReadShortByBigEndian(),
        Y0 = stream.ReadShortByBigEndian(),
        Radius0 = stream.ReadUShortByBigEndian(),
        X1 = stream.ReadShortByBigEndian(),
        Y1 = stream.ReadShortByBigEndian(),
        Radius1 = stream.ReadUShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(ColorLineOffset);
        stream.WriteShortByBigEndian(X0);
        stream.WriteShortByBigEndian(Y0);
        stream.WriteUShortByBigEndian(Radius0);
        stream.WriteShortByBigEndian(X1);
        stream.WriteShortByBigEndian(Y1);
        stream.WriteUShortByBigEndian(Radius1);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset sizeof(Offset24) */3 +
        X0.SizeOf() + Y0.SizeOf() +
        Radius0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        Radius1.SizeOf() +
        VarIndexBase.SizeOf();
}

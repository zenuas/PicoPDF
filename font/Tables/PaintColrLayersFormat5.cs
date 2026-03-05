using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrLayersFormat5 : IPaintColrLayersFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required ushort X0 { get; init; }
    public required ushort Y0 { get; init; }
    public required ushort X1 { get; init; }
    public required ushort Y1 { get; init; }
    public required ushort X2 { get; init; }
    public required ushort Y2 { get; init; }
    public required uint VarIndexBase { get; init; }

    public static PaintColrLayersFormat5 ReadFrom(Stream stream) => new()
    {
        Format = 5,
        ColorLineOffset = stream.Read3BytesByBigEndian(),
        X0 = stream.ReadUShortByBigEndian(),
        Y0 = stream.ReadUShortByBigEndian(),
        X1 = stream.ReadUShortByBigEndian(),
        Y1 = stream.ReadUShortByBigEndian(),
        X2 = stream.ReadUShortByBigEndian(),
        Y2 = stream.ReadUShortByBigEndian(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(ColorLineOffset);
        stream.WriteUShortByBigEndian(X0);
        stream.WriteUShortByBigEndian(Y0);
        stream.WriteUShortByBigEndian(X1);
        stream.WriteUShortByBigEndian(Y1);
        stream.WriteUShortByBigEndian(X2);
        stream.WriteUShortByBigEndian(Y2);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => Format.SizeOf() + ColorLineOffset.SizeOf() +
        X0.SizeOf() + Y0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        X2.SizeOf() + Y2.SizeOf() +
        VarIndexBase.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintRadialGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short X0 { get; init; }
    public required short Y0 { get; init; }
    public required ushort Radius0 { get; init; }
    public required short X1 { get; init; }
    public required short Y1 { get; init; }
    public required ushort Radius1 { get; init; }

    public static PaintRadialGradient ReadFrom(Stream stream) => new()
    {
        Format = 6,
        ColorLineOffset = stream.Read3BytesByBigEndian(),
        X0 = stream.ReadShortByBigEndian(),
        Y0 = stream.ReadShortByBigEndian(),
        Radius0 = stream.ReadUShortByBigEndian(),
        X1 = stream.ReadShortByBigEndian(),
        Y1 = stream.ReadShortByBigEndian(),
        Radius1 = stream.ReadUShortByBigEndian(),
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
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset.SizeOf() */Const.SizeofOffset24 +
        X0.SizeOf() + Y0.SizeOf() +
        Radius0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        Radius1.SizeOf();
}

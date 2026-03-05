using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrLayersFormat4 : IPaintColrLayersFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short X0 { get; init; }
    public required short Y0 { get; init; }
    public required short X1 { get; init; }
    public required short Y1 { get; init; }
    public required short X2 { get; init; }
    public required short Y2 { get; init; }

    public static PaintColrLayersFormat4 ReadFrom(Stream stream) => new()
    {
        Format = 4,
        ColorLineOffset = stream.Read3BytesByBigEndian(),
        X0 = stream.ReadShortByBigEndian(),
        Y0 = stream.ReadShortByBigEndian(),
        X1 = stream.ReadShortByBigEndian(),
        Y1 = stream.ReadShortByBigEndian(),
        X2 = stream.ReadShortByBigEndian(),
        Y2 = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(ColorLineOffset);
        stream.WriteShortByBigEndian(X0);
        stream.WriteShortByBigEndian(Y0);
        stream.WriteShortByBigEndian(X1);
        stream.WriteShortByBigEndian(Y1);
        stream.WriteShortByBigEndian(X2);
        stream.WriteShortByBigEndian(Y2);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset sizeof(Offset24) */3 +
        X0.SizeOf() + Y0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        X2.SizeOf() + Y2.SizeOf();
}

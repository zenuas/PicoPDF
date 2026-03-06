using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintLinearGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short X0 { get; init; }
    public required short Y0 { get; init; }
    public required short X1 { get; init; }
    public required short Y1 { get; init; }
    public required short X2 { get; init; }
    public required short Y2 { get; init; }

    public static PaintLinearGradient ReadFrom(Stream stream)
    {
        var colorLineOffset = stream.ReadOffset24();
        return new()
        {
            Format = 4,
            ColorLineOffset = colorLineOffset,
            X0 = stream.ReadFWORD(),
            Y0 = stream.ReadFWORD(),
            X1 = stream.ReadFWORD(),
            Y1 = stream.ReadFWORD(),
            X2 = stream.ReadFWORD(),
            Y2 = stream.ReadFWORD(),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(ColorLineOffset);
        stream.WriteFWORD(X0);
        stream.WriteFWORD(Y0);
        stream.WriteFWORD(X1);
        stream.WriteFWORD(Y1);
        stream.WriteFWORD(X2);
        stream.WriteFWORD(Y2);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset.SizeOf() */Const.SizeofOffset24 +
        X0.SizeOf() + Y0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        X2.SizeOf() + Y2.SizeOf();
}

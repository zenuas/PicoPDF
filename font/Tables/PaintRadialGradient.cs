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
    public required ColorLine ColorLine { get; init; }

    public static PaintRadialGradient ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var colorLineOffset = stream.ReadOffset24();
        return new()
        {
            Format = 6,
            ColorLineOffset = colorLineOffset,
            X0 = stream.ReadFWORD(),
            Y0 = stream.ReadFWORD(),
            Radius0 = stream.ReadUFWORD(),
            X1 = stream.ReadFWORD(),
            Y1 = stream.ReadFWORD(),
            Radius1 = stream.ReadUFWORD(),
            ColorLine = ColorLine.ReadFrom(stream.SeekTo(position + colorLineOffset)),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteFWORD(X0);
        stream.WriteFWORD(Y0);
        stream.WriteUFWORD(Radius0);
        stream.WriteFWORD(X1);
        stream.WriteFWORD(Y1);
        stream.WriteUFWORD(Radius1);
        ColorLine.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset.SizeOf() */Const.SizeofOffset24 +
        X0.SizeOf() + Y0.SizeOf() +
        Radius0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        Radius1.SizeOf();
}

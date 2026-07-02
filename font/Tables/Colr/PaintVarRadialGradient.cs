using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintVarRadialGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required Offset24 ColorLineOffset { get; init; }
    public required FWORD X0 { get; init; }
    public required FWORD Y0 { get; init; }
    public required UFWORD Radius0 { get; init; }
    public required FWORD X1 { get; init; }
    public required FWORD Y1 { get; init; }
    public required UFWORD Radius1 { get; init; }
    public required uint VarIndexBase { get; init; }
    public required VarColorLine ColorLine { get; init; }

    public static PaintVarRadialGradient ReadFrom(Stream stream, Dictionary<long, IColorLine> colorLineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var colorLineOffset = stream.ReadOffset24();
        return new()
        {
            Format = 7,
            ColorLineOffset = colorLineOffset,
            X0 = stream.ReadFWORD(),
            Y0 = stream.ReadFWORD(),
            Radius0 = stream.ReadUFWORD(),
            X1 = stream.ReadFWORD(),
            Y1 = stream.ReadFWORD(),
            Radius1 = stream.ReadUFWORD(),
            VarIndexBase = stream.ReadUIntByBigEndian(),
            ColorLine = VarColorLine.ReadFrom(stream, position + colorLineOffset.Value, colorLineCache),
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
        stream.WriteUIntByBigEndian(VarIndexBase);
        ColorLine.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset */Offset24.SizeOf() +
        X0.SizeOf() + Y0.SizeOf() +
        Radius0.SizeOf() +
        X1.SizeOf() + Y1.SizeOf() +
        Radius1.SizeOf() +
        VarIndexBase.SizeOf();
}

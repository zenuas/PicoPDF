using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintSweepGradient : IPaintFormat
{
    public required byte Format { get; init; }
    public required int ColorLineOffset { get; init; }
    public required short CenterX { get; init; }
    public required short CenterY { get; init; }
    public required ushort StartAngle { get; init; }
    public required ushort EndAngle { get; init; }
    public required ColorLine ColorLine { get; init; }

    public static PaintSweepGradient ReadFrom(Stream stream, Dictionary<long, IColorLine> colorLineCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var colorLineOffset = stream.ReadOffset24();
        return new()
        {
            Format = 8,
            ColorLineOffset = colorLineOffset,
            CenterX = stream.ReadFWORD(),
            CenterY = stream.ReadFWORD(),
            StartAngle = stream.ReadF2DOT14(),
            EndAngle = stream.ReadF2DOT14(),
            ColorLine = ColorLine.ReadFrom(stream, position + colorLineOffset, colorLineCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteFWORD(CenterX);
        stream.WriteFWORD(CenterY);
        stream.WriteF2DOT14(StartAngle);
        stream.WriteF2DOT14(EndAngle);
        ColorLine.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* ColorLineOffset */Const.SizeofOffset24 +
        CenterX.SizeOf() + CenterY.SizeOf() +
        StartAngle.SizeOf() + EndAngle.SizeOf();
}

using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintGlyph : IPaintFormat, IHaveGlyph, IHavePaint
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort GlyphID { get; init; }
    public required IPaintFormat Paint { get; init; }

    public static PaintGlyph ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache)
    {
        var position = stream.Position - /* sizeof(Format) */sizeof(byte);

        var paintOffset = stream.ReadOffset24();
        return new()
        {
            Format = 10,
            PaintOffset = paintOffset,
            GlyphID = stream.ReadUShortByBigEndian(),
            Paint = PaintFormat.ReadFrom(stream, position + paintOffset, paintCache),
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteOffset24(SizeOf());
        stream.WriteUShortByBigEndian(GlyphID);
        Paint.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset */Const.SizeofOffset24 + GlyphID.SizeOf();
}

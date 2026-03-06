using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintGlyph : IPaintFormat
{
    public required byte Format { get; init; }
    public required int PaintOffset { get; init; }
    public required ushort GlyphID { get; init; }

    public static PaintGlyph ReadFrom(Stream stream) => new()
    {
        Format = 10,
        PaintOffset = stream.Read3BytesByBigEndian(),
        GlyphID = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.Write3BytesByBigEndian(PaintOffset);
        stream.WriteUShortByBigEndian(GlyphID);
    }

    public int SizeOf() => Format.SizeOf() + /* PaintOffset.SizeOf() */Const.SizeofOffset24 + GlyphID.SizeOf();
}

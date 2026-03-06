using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class PaintColrGlyph : IPaintFormat
{
    public required byte Format { get; init; }
    public required ushort GlyphID { get; init; }

    public static PaintColrGlyph ReadFrom(Stream stream) => new()
    {
        Format = 11,
        GlyphID = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteUShortByBigEndian(GlyphID);
    }

    public int SizeOf() => Format.SizeOf() + GlyphID.SizeOf();
}

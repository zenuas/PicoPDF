using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class BaseGlyphListRecord : IExportable
{
    public required uint NumberBaseGlyphPaintRecords { get; init; }
    public required (ushort GlyphID, uint PaintOffset)[] BaseGlyphPaintRecord { get; init; }

    public static BaseGlyphListRecord ReadFrom(Stream stream)
    {
        var numBaseGlyphPaintRecords = stream.ReadUIntByBigEndian();

        return new()
        {
            NumberBaseGlyphPaintRecords = numBaseGlyphPaintRecords,
            BaseGlyphPaintRecord = [.. Lists.Repeat(() => (stream.ReadUShortByBigEndian(), stream.ReadUIntByBigEndian())).Take((int)numBaseGlyphPaintRecords)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian((uint)BaseGlyphPaintRecord.Length);
        foreach (var x in BaseGlyphPaintRecord)
        {
            stream.WriteUShortByBigEndian(x.GlyphID);
            stream.WriteUIntByBigEndian(x.PaintOffset);
        }
    }

    public int SizeOf() => NumberBaseGlyphPaintRecords.SizeOf() + ((sizeof(ushort) + sizeof(uint)) * BaseGlyphPaintRecord.Length);
}

using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class BaseGlyphListRecord
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
        stream.WriteUIntByBigEndian(NumberBaseGlyphPaintRecords);
        foreach (var x in BaseGlyphPaintRecord)
        {
            stream.WriteUShortByBigEndian(x.GlyphID);
            stream.WriteUIntByBigEndian(x.PaintOffset);
        }
    }
}

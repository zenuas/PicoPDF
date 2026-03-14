using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class BaseGlyphListRecord : IExportable
{
    public required uint NumberBaseGlyphPaintRecords { get; init; }
    public required (ushort GlyphID, uint PaintOffset)[] BaseGlyphPaintRecord { get; init; }
    public required IPaintFormat[] Paints { get; init; }

    public static BaseGlyphListRecord ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache)
    {
        var position = stream.Position;

        var numBaseGlyphPaintRecords = stream.ReadUIntByBigEndian();
        var baseGlyphPaintRecord = Lists.Repeat(() => (GlyphID: stream.ReadUShortByBigEndian(), PaintOffset: stream.ReadUIntByBigEndian())).Take((int)numBaseGlyphPaintRecords).ToArray();

        return new()
        {
            NumberBaseGlyphPaintRecords = numBaseGlyphPaintRecords,
            BaseGlyphPaintRecord = baseGlyphPaintRecord,
            Paints = [.. baseGlyphPaintRecord.Select(x => PaintFormat.ReadFrom(stream, position + x.PaintOffset, paintCache, colorLineCache, affineCache))],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian((uint)BaseGlyphPaintRecord.Length);

        using var mem = new MemoryStream();
        for (var i = 0; i < BaseGlyphPaintRecord.Length; i++)
        {
            stream.WriteUShortByBigEndian(BaseGlyphPaintRecord[i].GlyphID);
            stream.WriteUIntByBigEndian((uint)(SizeOf() + mem.Length));
            Paints[i].WriteTo(mem);
        }
        stream.Write(mem.ToArray());
    }

    public int SizeOf() => NumberBaseGlyphPaintRecords.SizeOf() + ((sizeof(ushort) + sizeof(uint)) * BaseGlyphPaintRecord.Length);
}

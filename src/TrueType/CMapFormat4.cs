using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public struct CMapFormat4
{
    public ushort Format;
    public ushort Length;
    public ushort Language;
    public ushort SegCountX2;
    public ushort SearchRange;
    public ushort EntrySelector;
    public ushort RangeShift;
    public ushort[] EndCode;
    public ushort ReservedPad;
    public ushort[] StartCode;
    public short[] IdDelta;
    public ushort[] IdRangeOffsets;
    public ushort[] GlyphIdArray;

    public static CMapFormat4 ReadFrom(Stream stream, TableRecord rec, EncodingRecord enc)
    {
        stream.Position = rec.Offset + enc.Offset;
        var cmap4 = new CMapFormat4()
        {
            Format = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            Length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            Language = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            SegCountX2 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        };

        var seg_count = cmap4.SegCountX2 / 2;
        var glyph_count = (cmap4.Length - (16 + (8 * seg_count))) / 2;

        cmap4.EndCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        cmap4.ReservedPad = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        cmap4.StartCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        cmap4.IdDelta = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray();
        cmap4.IdRangeOffsets = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        cmap4.GlyphIdArray = Lists.RangeTo(0, glyph_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();

        return cmap4;
    }
}

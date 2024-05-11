using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public class CMapFormat4
{
    public readonly ushort Format;
    public readonly ushort Length;
    public readonly ushort Language;
    public readonly ushort SegCountX2;
    public readonly ushort SearchRange;
    public readonly ushort EntrySelector;
    public readonly ushort RangeShift;
    public readonly ushort[] EndCode;
    public readonly ushort ReservedPad;
    public readonly ushort[] StartCode;
    public readonly short[] IdDelta;
    public readonly ushort[] IdRangeOffsets;
    public readonly ushort[] GlyphIdArray;

    public CMapFormat4(Stream stream, TableRecord rec, EncodingRecord enc)
    {
        stream.Position = rec.Offset + enc.Offset;

        Format = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        Length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        Language = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        SegCountX2 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        var seg_count = SegCountX2 / 2;
        var glyph_count = (Length - (16 + (8 * seg_count))) / 2;

        EndCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        ReservedPad = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        StartCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        IdDelta = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray();
        IdRangeOffsets = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        GlyphIdArray = Lists.RangeTo(0, glyph_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
    }
}

using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class CMapFormat4 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required ushort SegCountX2 { get; init; }
    public required ushort SearchRange { get; init; }
    public required ushort EntrySelector { get; init; }
    public required ushort RangeShift { get; init; }
    public required ushort[] EndCode { get; init; }
    public required ushort ReservedPad { get; init; }
    public required ushort[] StartCode { get; init; }
    public required short[] IdDelta { get; init; }
    public required ushort[] IdRangeOffsets { get; init; }
    public required ushort[] GlyphIdArray { get; init; }

    public static CMapFormat4 ReadFrom(Stream stream)
    {
        var length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var language = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var seg_count_x2 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        var seg_count = seg_count_x2 / 2;
        var glyph_count = (length - (16 + (8 * seg_count))) / 2;

        return new()
        {
            Format = 4,
            Length = length,
            Language = language,
            SegCountX2 = seg_count_x2,
            SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            EndCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            ReservedPad = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            StartCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            IdDelta = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            IdRangeOffsets = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            GlyphIdArray = Lists.RangeTo(0, glyph_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
        };
    }
}

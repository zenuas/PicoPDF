using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class PairValue
{
    public required ushort SecondGlyph { get; init; }
    public required ValueRecord ValueRecord1 { get; init; }
    public required ValueRecord ValueRecord2 { get; init; }

    public static PairValue ReadFrom(Stream stream, ValueFormatFlags value_format1, ValueFormatFlags value_format2)
    {
        var second_glyph = stream.ReadUShortByBigEndian();

        var position = stream.Position;

        var valuerecord1 = ValueRecord.ReadFrom(stream, value_format1);
        var valuerecord2 = ValueRecord.ReadFrom(stream.SeekTo(position + ValueRecord.LoadSize(value_format1)), value_format2);

        return new()
        {
            SecondGlyph = second_glyph,
            ValueRecord1 = valuerecord1,
            ValueRecord2 = valuerecord2,
        };
    }

    public int SizeOf() => SecondGlyph.SizeOf() + ValueRecord1.SizeOf() + ValueRecord2.SizeOf();

    public static int LoadSize(ValueFormatFlags value_format1, ValueFormatFlags value_format2) => sizeof(ushort) + ValueRecord.LoadSize(value_format1) + ValueRecord.LoadSize(value_format2);
}

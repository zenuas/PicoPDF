using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class SinglePosFormat2 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ValueFormatFlags ValueFormat { get; init; }
    public required ushort ValueCount { get; init; }
    public required ValueRecord[] ValueRecords { get; init; }
    public required ICoverageFormat Coverage { get; init; }

    public static SinglePosFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var value_format = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var value_count = stream.ReadUShortByBigEndian();
        var value_record = Lists.Repeat(() => ValueRecord.ReadFrom(stream, value_format)).Take(value_count).ToArray();

        _ = stream.Seek(position + coverage_offset.Value, SeekOrigin.Begin);
        var coverage_format = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 2,
            CoverageOffset = coverage_offset,
            ValueFormat = value_format,
            ValueCount = value_count,
            ValueRecords = value_record,
            Coverage = coverage_format switch
            {
                1 => CoverageFormat1.ReadFrom(stream),
                2 => CoverageFormat2.ReadFrom(stream),
                _ => throw new(),
            },
        };
    }
}

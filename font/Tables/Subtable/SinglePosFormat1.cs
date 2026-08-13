using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class SinglePosFormat1 : ISubtable, ISinglePosition
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ValueFormatFlags ValueFormat { get; init; }
    public required ValueRecord ValueRecord { get; init; }
    public required ICoverageFormat Coverage { get; init; }

    public static SinglePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var value_format = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var value_record = ValueRecord.ReadFrom(stream, value_format);

        _ = stream.Seek(position + coverage_offset.Value, SeekOrigin.Begin);
        var coverage_format = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            ValueFormat = value_format,
            ValueRecord = value_record,
            Coverage = coverage_format switch
            {
                1 => CoverageFormat1.ReadFrom(stream),
                2 => CoverageFormat2.ReadFrom(stream),
                _ => throw new(),
            },
        };
    }

    public ValueRecord? GetPosition(uint gid) => Coverage.FindOrNull(gid) is { } ? ValueRecord : null;
}

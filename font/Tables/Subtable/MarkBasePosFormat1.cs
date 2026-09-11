using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class MarkBasePosFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 MarkCoverageOffset { get; init; }
    public required Offset16 BaseCoverageOffset { get; init; }
    public required ushort MarkClassCount { get; init; }
    public required Offset16 MarkArrayOffset { get; init; }
    public required Offset16 BaseArrayOffset { get; init; }
    public required ICoverageFormat? MarkCoverage { get; init; }
    public required ICoverageFormat? BaseCoverage { get; init; }
    public required MarkArrayTable MarkArray { get; init; }
    public required BaseArrayTable BaseArray { get; init; }

    public static MarkBasePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var mark_coverage_offset = stream.ReadOffset16();
        var base_coverage_offset = stream.ReadOffset16();
        var mark_class_count = stream.ReadUShortByBigEndian();
        var mark_array_offset = stream.ReadOffset16();
        var base_array_offset = stream.ReadOffset16();

        return new()
        {
            Format = 1,
            MarkCoverageOffset = mark_coverage_offset,
            BaseCoverageOffset = base_coverage_offset,
            MarkClassCount = mark_class_count,
            MarkArrayOffset = mark_array_offset,
            BaseArrayOffset = base_array_offset,
            MarkCoverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + mark_coverage_offset.Value)),
            BaseCoverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + base_coverage_offset.Value)),
            MarkArray = MarkArrayTable.ReadFrom(stream.SeekTo(position + mark_array_offset.Value)),
            BaseArray = BaseArrayTable.ReadFrom(stream.SeekTo(position + base_array_offset.Value), mark_class_count),
        };
    }
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class CursivePosFormat1 : ISubtable
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ushort EntryExitCount { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required EntryExitRecord[] EntryExitRecords { get; init; }

    public static CursivePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var entry_exit_count = stream.ReadUShortByBigEndian();

        var entry_exit_records_offset = stream.Position;

        return new()
        {
            Format = 1,
            CoverageOffset = coverage_offset,
            EntryExitCount = entry_exit_count,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            EntryExitRecords = [.. Lists.Sequence(entry_exit_records_offset, EntryExitRecord.SizeOf()).Select(x => EntryExitRecord.ReadFrom(stream.SeekTo(x))).Take(entry_exit_count)],
        };
    }
}

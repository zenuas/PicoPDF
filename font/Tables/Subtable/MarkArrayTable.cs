using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class MarkArrayTable
{
    public required ushort MarkCount { get; init; }
    public required MarkRecord[] MarkRecords { get; init; }

    public static MarkArrayTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var mark_count = stream.ReadUShortByBigEndian();

        var mark_records_offset = stream.Position;

        return new()
        {
            MarkCount = mark_count,
            MarkRecords = [.. Lists.Sequence(mark_records_offset, MarkRecord.SizeOf()).Select(x => MarkRecord.ReadFrom(stream.SeekTo(x), position)).Take(mark_count)],
        };
    }
}

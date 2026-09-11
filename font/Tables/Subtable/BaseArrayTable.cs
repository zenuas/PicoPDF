using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class BaseArrayTable
{
    public required ushort BaseCount { get; init; }
    public required BaseRecord[] BaseRecords { get; init; }

    public static BaseArrayTable ReadFrom(Stream stream, ushort mark_class_count)
    {
        var position = stream.Position;

        var base_count = stream.ReadUShortByBigEndian();

        var base_records_offset = stream.Position;

        return new()
        {
            BaseCount = base_count,
            BaseRecords = [.. Lists.Sequence(base_records_offset, BaseRecord.SizeOf(mark_class_count)).Select(x => BaseRecord.ReadFrom(stream.SeekTo(x), position, mark_class_count)).Take(base_count)],
        };
    }
}

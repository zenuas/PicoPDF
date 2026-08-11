using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class LookupListRecord
{
    public required ushort LookupCount { get; init; }
    public required (Offset16 LookupOffset, LookupTableRecord LookupTable)[] LookupRecords { get; init; }

    public static LookupListRecord ReadFrom(Stream stream, TableTypes table_type)
    {
        var position = stream.Position;

        var lookup_count = stream.ReadUShortByBigEndian();
        var lookup_records = Lists.Repeat(stream.ReadOffset16).Take(lookup_count).ToArray();

        return new()
        {
            LookupCount = lookup_count,
            LookupRecords = [.. lookup_records.Select(x => (x, LookupTableRecord.ReadFrom(stream.SeekTo(position + x.Value), table_type)))],
        };
    }

    public int SizeOf() => LookupCount.SizeOf() + (Offset16.SizeOf() * LookupRecords.Length);
}

using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class LookupListTable
{
    public required ushort LookupCount { get; init; }
    public required Offset16[] LookupOffsets { get; init; }
    public required LookupTable[] Lookups { get; init; }

    public static LookupListTable ReadFrom(Stream stream, TableTypes table_type)
    {
        var position = stream.Position;

        var lookup_count = stream.ReadUShortByBigEndian();
        var lookup_offsets = Lists.Repeat(stream.ReadOffset16).Take(lookup_count).ToArray();

        return new()
        {
            LookupCount = lookup_count,
            LookupOffsets = lookup_offsets,
            Lookups = [.. lookup_offsets.Select(x => LookupTable.ReadFrom(stream.SeekTo(position + x.Value), table_type))],
        };
    }

    public int SizeOf() => LookupCount.SizeOf() + (Offset16.SizeOf() * LookupOffsets.Length);
}

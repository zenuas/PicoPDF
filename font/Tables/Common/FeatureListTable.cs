using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class FeatureListTable
{
    public required ushort FeatureCount { get; init; }
    public required FeatureRecord[] FeatureRecords { get; init; }

    public static FeatureListTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var feature_count = stream.ReadUShortByBigEndian();

        var feature_records_offset = stream.Position;

        return new()
        {
            FeatureCount = feature_count,
            FeatureRecords = [.. Lists.Sequence(feature_records_offset, FeatureRecord.SizeOf()).Select(x => FeatureRecord.ReadFrom(stream.SeekTo(x), position)).Take(feature_count)],
        };
    }

    public int SizeOf() => FeatureCount.SizeOf() + (FeatureRecord.SizeOf() * FeatureRecords.Length);
}

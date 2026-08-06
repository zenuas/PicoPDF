using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType.Tables.Common;

public class FeatureListRecord
{
    public required ushort FeatureCount { get; init; }
    public required (string FeatureTag, Offset16 FeatureOffset, FeatureTableRecord FeatureTable)[] FeatureRecords { get; init; }

    public static FeatureListRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var feature_count = stream.ReadUShortByBigEndian();
        var feature_records = Lists.Repeat(() => (FeatureTag: Encoding.ASCII.GetString(stream.ReadExactly(4)), FeatureOffset: stream.ReadOffset16())).Take(feature_count).ToArray();

        return new()
        {
            FeatureCount = feature_count,
            FeatureRecords = [.. feature_records.Select(x => (x.FeatureTag, x.FeatureOffset, FeatureTableRecord.ReadFrom(stream.SeekTo(position + x.FeatureOffset.Value))))],
        };
    }

    public int SizeOf() => FeatureCount.SizeOf() + ((/* sizeof(FeatureTag) */4 + Offset16.SizeOf()) * FeatureRecords.Length);
}

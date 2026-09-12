using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Text;

namespace OpenType.Tables.Common;

public class FeatureRecord
{
    public required string FeatureTag { get; init; }
    public required Offset16 FeatureOffset { get; init; }
    public required FeatureTable FeatureTable { get; init; }

    public static FeatureRecord ReadFrom(Stream stream, long feature_list_offset)
    {
        var feature_tag = Encoding.ASCII.GetString(stream.ReadExactly(4));
        var feature_offset = stream.ReadOffset16();

        return new()
        {
            FeatureTag = feature_tag,
            FeatureOffset = feature_offset,
            // Offset to Feature table, from beginning of FeatureList.
            FeatureTable = FeatureTable.ReadFrom(stream.SeekTo(feature_list_offset + feature_offset.Value)),
        };
    }

    public static int SizeOf() => /* sizeof(FeatureTag) */4 + /* sizeof(FeatureOffset) */Offset16.SizeOf();
}

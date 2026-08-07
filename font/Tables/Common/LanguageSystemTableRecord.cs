using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class LanguageSystemTableRecord
{
    public required Offset16 LookupOrderOffset { get; init; }
    public required ushort RequiredFeatureIndex { get; init; }
    public required ushort FeatureIndexCount { get; init; }
    public required ushort[] FeatureIndices { get; init; }

    public static LanguageSystemTableRecord ReadFrom(Stream stream)
    {
        var lookup_order_offset = stream.ReadOffset16();
        var required_feature_index = stream.ReadUShortByBigEndian();
        var feature_index_count = stream.ReadUShortByBigEndian();

        return new()
        {
            LookupOrderOffset = lookup_order_offset,
            RequiredFeatureIndex = required_feature_index,
            FeatureIndexCount = feature_index_count,
            FeatureIndices = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(feature_index_count)],
        };
    }

    public int SizeOf() => LookupOrderOffset.SizeOf() + RequiredFeatureIndex.SizeOf() + FeatureIndexCount.SizeOf() + (sizeof(ushort) * FeatureIndices.Length);
}

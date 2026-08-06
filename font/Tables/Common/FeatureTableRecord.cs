using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class FeatureTableRecord
{
    public required Offset16 FeatureParamsOffset { get; init; }
    public required ushort LookupIndexCount { get; init; }
    public required ushort[] LookupListIndices { get; init; }

    public static FeatureTableRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var feature_params_offset = stream.ReadOffset16();
        var lookup_index_count = stream.ReadUShortByBigEndian();

        return new()
        {
            FeatureParamsOffset = feature_params_offset,
            LookupIndexCount = lookup_index_count,
            LookupListIndices = [.. Lists.Repeat(() => stream.ReadUShortByBigEndian()).Take(lookup_index_count)]
        };
    }

    public int SizeOf() => FeatureParamsOffset.SizeOf() + LookupIndexCount.SizeOf() + (sizeof(ushort) * LookupListIndices.Length);
}

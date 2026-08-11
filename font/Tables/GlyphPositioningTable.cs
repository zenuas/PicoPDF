using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables.Common;
using System.IO;

namespace OpenType.Tables;

public record class GlyphPositioningTable : IExportable
{
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required Offset16 ScriptListOffset { get; init; }
    public required Offset16 FeatureListOffset { get; init; }
    public required Offset16 LookupListOffset { get; init; }
    public required Offset32 FeatureVariationsOffset { get; init; }
    public required ScriptListRecord? ScriptList { get; init; }
    public required FeatureListRecord? FeatureList { get; init; }
    public required LookupListRecord? LookupList { get; init; }

    public static GlyphPositioningTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major_version = stream.ReadUShortByBigEndian();
        var minor_version = stream.ReadUShortByBigEndian();
        var script_list_offset = stream.ReadOffset16();
        var feature_list_offset = stream.ReadOffset16();
        var lookup_list_offset = stream.ReadOffset16();
        var feature_variations_offset = major_version == 1 && minor_version == 1 ? stream.ReadOffset32() : 0;

        return new()
        {
            MajorVersion = major_version,
            MinorVersion = minor_version,
            ScriptListOffset = script_list_offset,
            FeatureListOffset = feature_list_offset,
            LookupListOffset = lookup_list_offset,
            FeatureVariationsOffset = feature_variations_offset,
            ScriptList = script_list_offset.Value == 0 ? null : ScriptListRecord.ReadFrom(stream.SeekTo(position + script_list_offset.Value)),
            FeatureList = feature_list_offset.Value == 0 ? null : FeatureListRecord.ReadFrom(stream.SeekTo(position + feature_list_offset.Value)),
            LookupList = lookup_list_offset.Value == 0 ? null : LookupListRecord.ReadFrom(stream.SeekTo(position + lookup_list_offset.Value), TableTypes.GPOS)
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(MajorVersion);
        stream.WriteUShortByBigEndian(MinorVersion);
        stream.WriteOffset16(ScriptListOffset);
        stream.WriteOffset16(FeatureListOffset);
        stream.WriteOffset16(LookupListOffset);
        if (MajorVersion == 1 && MinorVersion == 1) stream.WriteOffset32(FeatureVariationsOffset);
    }
}

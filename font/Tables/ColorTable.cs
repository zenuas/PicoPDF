using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables.Colr;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public record class ColorTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberBaseGlyphRecords { get; init; }
    public required Offset32 BaseGlyphRecordsOffset { get; init; }
    public required Offset32 LayerRecordsOffset { get; init; }
    public required ushort NumberLayerRecords { get; init; }
    public required BaseGlyphRecord[] BaseGlyphRecords { get; init; }
    public required LayerRecord[] LayerRecords { get; init; }
    public required BaseGlyphListRecord? BaseGlyphListRecord { get; init; }
    public required LayerListRecord? LayerListRecord { get; init; }
    public required ClipListRecord? ClipListRecord { get; init; }
    public required DeltaSetIndexMapRecord? DeltaSetIndexMapRecord { get; init; }
    public required ItemVariationStoreRecord? ItemVariationStoreRecord { get; init; }

    public static ColorTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var version = stream.ReadUShortByBigEndian();
        var numBaseGlyphRecords = stream.ReadUShortByBigEndian();
        var baseGlyphRecordsOffset = stream.ReadOffset32();
        var layerRecordsOffset = stream.ReadOffset32();
        var numLayerRecords = stream.ReadUShortByBigEndian();

        Offset32 baseGlyphListOffset = 0;
        Offset32 layerListOffset = 0;
        Offset32 clipListOffset = 0;
        Offset32 varIndexMapOffset = 0;
        Offset32 itemVariationStoreOffset = 0;
        if (version >= 1)
        {
            baseGlyphListOffset = stream.ReadOffset32();
            layerListOffset = stream.ReadOffset32();
            clipListOffset = stream.ReadOffset32();
            varIndexMapOffset = stream.ReadOffset32();
            itemVariationStoreOffset = stream.ReadOffset32();
        }

        var baseGlyphRecords = baseGlyphRecordsOffset.Value == 0 || numBaseGlyphRecords == 0 ? [] :
            stream.SeekTo(position + baseGlyphRecordsOffset.Value).To(_ => Lists.Repeat(() => BaseGlyphRecord.ReadFrom(stream)).Take(numBaseGlyphRecords).ToArray());

        var layerRecords = layerRecordsOffset.Value == 0 || numLayerRecords == 0 ? [] :
            stream.SeekTo(position + layerRecordsOffset.Value).To(_ => Lists.Repeat(() => LayerRecord.ReadFrom(stream)).Take(numLayerRecords).ToArray());

        var paintCache = new Dictionary<long, IPaintFormat>();
        var colorLineCache = new Dictionary<long, IColorLine>();
        var affineCache = new Dictionary<long, IAffine2x3>();

        var baseGlyphList = baseGlyphListOffset.Value == 0 ? null
            : stream.SeekTo(position + baseGlyphListOffset.Value).To(x => BaseGlyphListRecord.ReadFrom(x, paintCache, colorLineCache, affineCache));

        var layerList = layerListOffset.Value == 0 ? null
            : stream.SeekTo(position + layerListOffset.Value).To(x => LayerListRecord.ReadFrom(x, paintCache, colorLineCache, affineCache));

        var clipList = clipListOffset.Value == 0 ? null
            : stream.SeekTo(position + clipListOffset.Value).To(ClipListRecord.ReadFrom);

        var varIndexMap = varIndexMapOffset.Value == 0 ? null
            : stream.SeekTo(position + varIndexMapOffset.Value).To(DeltaSetIndexMapRecord.ReadFrom);

        var itemVariationStore = itemVariationStoreOffset.Value == 0 ? null
            : stream.SeekTo(position + itemVariationStoreOffset.Value).To(ItemVariationStoreRecord.ReadFrom);

        return new()
        {
            Version = version,
            NumberBaseGlyphRecords = numBaseGlyphRecords,
            BaseGlyphRecordsOffset = baseGlyphRecordsOffset,
            LayerRecordsOffset = layerRecordsOffset,
            NumberLayerRecords = numLayerRecords,
            BaseGlyphRecords = baseGlyphRecords,
            LayerRecords = layerRecords,
            BaseGlyphListRecord = baseGlyphList,
            LayerListRecord = layerList,
            ClipListRecord = clipList,
            DeltaSetIndexMapRecord = varIndexMap,
            ItemVariationStoreRecord = itemVariationStore,
        };
    }

    public void WriteTo(Stream stream)
    {
        var offset = Version.SizeOf() + NumberBaseGlyphRecords.SizeOf() + BaseGlyphRecordsOffset.SizeOf() + LayerRecordsOffset.SizeOf() + NumberLayerRecords.SizeOf() + (Version >= 1 ? sizeof(uint) * 5 : 0);
        var sizeof_BaseGlyphRecords = BaseGlyphRecords.Select(x => x.SizeOf()).Sum();
        var sizeof_LayerRecords = LayerRecords.Select(x => x.SizeOf()).Sum();

        stream.WriteUShortByBigEndian(Version);
        stream.WriteUShortByBigEndian((ushort)BaseGlyphRecords.Length);
        stream.WriteOffset32(BaseGlyphRecords.Length == 0 ? 0 : offset);
        stream.WriteOffset32(LayerRecords.Length == 0 ? 0 : offset + sizeof_BaseGlyphRecords);
        stream.WriteUShortByBigEndian((ushort)LayerRecords.Length);

        offset += sizeof_BaseGlyphRecords + sizeof_LayerRecords;
        using var baseGlyphList = new MemoryStream();
        using var layerList = new MemoryStream();
        if (Version >= 1)
        {
            if (BaseGlyphListRecord is { })
            {
                stream.WriteOffset32(offset);
                BaseGlyphListRecord.WriteTo(baseGlyphList);
                offset += (int)baseGlyphList.Length;
            }
            else
            {
                stream.WriteUIntByBigEndian(0);
            }

            if (LayerListRecord is { })
            {
                stream.WriteOffset32(offset);
                LayerListRecord.WriteTo(layerList);
                offset += (int)layerList.Length;

            }
            else
            {
                stream.WriteUIntByBigEndian(0);
            }
            stream.WriteOffset32(ClipListRecord is { } ? offset : 0); offset += ClipListRecord?.SizeOf() ?? 0;
            stream.WriteOffset32(DeltaSetIndexMapRecord is { } ? offset : 0); offset += DeltaSetIndexMapRecord?.SizeOf() ?? 0;
            stream.WriteOffset32(ItemVariationStoreRecord is { } ? offset : 0); offset += ItemVariationStoreRecord?.SizeOf() ?? 0;
        }

        BaseGlyphRecords.Each(x => x.WriteTo(stream));
        LayerRecords.Each(x => x.WriteTo(stream));
        if (Version >= 1)
        {
            if (BaseGlyphListRecord is { }) stream.Write(baseGlyphList.ToArray());
            if (LayerListRecord is { }) stream.Write(layerList.ToArray());
            if (ClipListRecord is { }) ClipListRecord.WriteTo(stream);
            if (DeltaSetIndexMapRecord is { }) DeltaSetIndexMapRecord.WriteTo(stream);
            if (ItemVariationStoreRecord is { }) ItemVariationStoreRecord.WriteTo(stream);
        }
    }
}

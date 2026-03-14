using Mina.Extension;
using OpenType.Tables.Colr;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class ColorTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberBaseGlyphRecords { get; init; }
    public required uint BaseGlyphRecordsOffset { get; init; }
    public required uint LayerRecordsOffset { get; init; }
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
        var baseGlyphRecordsOffset = stream.ReadUIntByBigEndian();
        var layerRecordsOffset = stream.ReadUIntByBigEndian();
        var numLayerRecords = stream.ReadUShortByBigEndian();

        uint baseGlyphListOffset = 0;
        uint layerListOffset = 0;
        uint clipListOffset = 0;
        uint varIndexMapOffset = 0;
        uint itemVariationStoreOffset = 0;
        if (version >= 1)
        {
            baseGlyphListOffset = stream.ReadUIntByBigEndian();
            layerListOffset = stream.ReadUIntByBigEndian();
            clipListOffset = stream.ReadUIntByBigEndian();
            varIndexMapOffset = stream.ReadUIntByBigEndian();
            itemVariationStoreOffset = stream.ReadUIntByBigEndian();
        }

        var paintCache = new Dictionary<long, IPaintFormat>();

        var baseGlyphRecords = baseGlyphRecordsOffset == 0 || numBaseGlyphRecords == 0 ? [] :
            stream.SeekTo(position + baseGlyphRecordsOffset).To(_ => Lists.Repeat(() => BaseGlyphRecord.ReadFrom(stream)).Take(numBaseGlyphRecords).ToArray());

        var layerRecords = layerRecordsOffset == 0 || numLayerRecords == 0 ? [] :
            stream.SeekTo(position + layerRecordsOffset).To(_ => Lists.Repeat(() => LayerRecord.ReadFrom(stream)).Take(numLayerRecords).ToArray());

        var baseGlyphList = baseGlyphListOffset == 0 ? null
            : stream.SeekTo(position + baseGlyphListOffset).To(x => BaseGlyphListRecord.ReadFrom(x, paintCache));

        var layerList = layerListOffset == 0 ? null
            : stream.SeekTo(position + layerListOffset).To(x => LayerListRecord.ReadFrom(x, paintCache));

        var clipList = clipListOffset == 0 ? null
            : stream.SeekTo(position + clipListOffset).To(ClipListRecord.ReadFrom);

        var varIndexMap = varIndexMapOffset == 0 ? null
            : stream.SeekTo(position + varIndexMapOffset).To(DeltaSetIndexMapRecord.ReadFrom);

        var itemVariationStore = itemVariationStoreOffset == 0 ? null
            : stream.SeekTo(position + itemVariationStoreOffset).To(ItemVariationStoreRecord.ReadFrom);

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
        stream.WriteUIntByBigEndian(BaseGlyphRecords.Length == 0 ? 0 : (uint)offset);
        stream.WriteUIntByBigEndian(LayerRecords.Length == 0 ? 0 : (uint)(offset + sizeof_BaseGlyphRecords));
        stream.WriteUShortByBigEndian((ushort)LayerRecords.Length);

        offset += sizeof_BaseGlyphRecords + sizeof_LayerRecords;
        using var baseGlyphList = new MemoryStream();
        using var layerList = new MemoryStream();
        if (Version >= 1)
        {
            if (BaseGlyphListRecord is { })
            {
                stream.WriteUIntByBigEndian((uint)offset);
                BaseGlyphListRecord.WriteTo(baseGlyphList);
                offset += (int)baseGlyphList.Length;
            }
            else
            {
                stream.WriteUIntByBigEndian(0);
            }

            if (LayerListRecord is { })
            {
                stream.WriteUIntByBigEndian((uint)offset);
                LayerListRecord.WriteTo(layerList);
                offset += (int)layerList.Length;

            }
            else
            {
                stream.WriteUIntByBigEndian(0);
            }
            stream.WriteUIntByBigEndian(ClipListRecord is { } ? (uint)offset : 0); offset += ClipListRecord?.SizeOf() ?? 0;
            stream.WriteUIntByBigEndian(DeltaSetIndexMapRecord is { } ? (uint)offset : 0); offset += DeltaSetIndexMapRecord?.SizeOf() ?? 0;
            stream.WriteUIntByBigEndian(ItemVariationStoreRecord is { } ? (uint)offset : 0); offset += ItemVariationStoreRecord?.SizeOf() ?? 0;
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

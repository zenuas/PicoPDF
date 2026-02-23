using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

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

        var baseGlyphRecords = stream.SeekTo(position + baseGlyphRecordsOffset).To(_ => Lists.Repeat(() => BaseGlyphRecord.ReadFrom(stream)).Take(numBaseGlyphRecords).ToArray());
        var layerRecords = stream.SeekTo(position + layerRecordsOffset).To(_ => Lists.Repeat(() => LayerRecord.ReadFrom(stream)).Take(numLayerRecords).ToArray());

        var baseGlyphList = baseGlyphListOffset == 0 ? null
            : stream.SeekTo(position + baseGlyphListOffset).To(BaseGlyphListRecord.ReadFrom);

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
        };
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}

using Mina.Extension;
using System;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class ColorTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberBaseGlyphRecords { get; init; }
    public required uint BaseGlyphRecordsOffset { get; init; }
    public required uint LayerRecordsOffset { get; init; }
    public required ushort NumberLayerRecords { get; init; }


    public static ColorTable ReadFrom(Stream stream)
    {
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

        return new()
        {
            Version = version,
            NumberBaseGlyphRecords = numBaseGlyphRecords,
            BaseGlyphRecordsOffset = baseGlyphRecordsOffset,
            LayerRecordsOffset = layerRecordsOffset,
            NumberLayerRecords = numLayerRecords,
        };
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}

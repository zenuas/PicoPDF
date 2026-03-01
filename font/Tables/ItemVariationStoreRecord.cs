using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class ItemVariationStoreRecord
{
    public required ushort Format { get; init; }
    public required uint VariationRegionListOffset { get; init; }
    public required ushort ItemVariationDataCount { get; init; }
    public required uint[] ItemVariationDataOffsets { get; init; }

    public static ItemVariationStoreRecord ReadFrom(Stream stream)
    {
        var format = stream.ReadUShortByBigEndian();
        var variationRegionListOffset = stream.ReadUIntByBigEndian();
        var itemVariationDataCount = stream.ReadUShortByBigEndian();
        var itemVariationDataOffsets = Lists.Repeat(stream.ReadUIntByBigEndian).Take((int)itemVariationDataCount).ToArray();

        return new()
        {
            Format = format,
            VariationRegionListOffset = variationRegionListOffset,
            ItemVariationDataCount = itemVariationDataCount,
            ItemVariationDataOffsets = itemVariationDataOffsets
        };
    }

    public void WriteTo(Stream stream)
    {
    }
}

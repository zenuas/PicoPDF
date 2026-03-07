using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class ItemVariationStoreRecord : IExportable
{
    public required ushort Format { get; init; }
    public required uint VariationRegionListOffset { get; init; }
    public required ushort ItemVariationDataCount { get; init; }
    public required uint[] ItemVariationDataOffsets { get; init; }
    public required VariationRegionListRecord? VariationRegionListRecord { get; init; }

    public static ItemVariationStoreRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var format = stream.ReadUShortByBigEndian();
        var variationRegionListOffset = stream.ReadUIntByBigEndian();
        var itemVariationDataCount = stream.ReadUShortByBigEndian();
        var itemVariationDataOffsets = Lists.Repeat(stream.ReadUIntByBigEndian).Take((int)itemVariationDataCount).ToArray();

        var variationRegionList = variationRegionListOffset == 0 ? null
            : stream.SeekTo(position + variationRegionListOffset).To(VariationRegionListRecord.ReadFrom);

        return new()
        {
            Format = format,
            VariationRegionListOffset = variationRegionListOffset,
            ItemVariationDataCount = itemVariationDataCount,
            ItemVariationDataOffsets = itemVariationDataOffsets,
            VariationRegionListRecord = variationRegionList,
        };
    }

    public void WriteTo(Stream stream)
    {
        var offset = Format.SizeOf() + VariationRegionListOffset.SizeOf() + ItemVariationDataCount.SizeOf() + (sizeof(uint) * ItemVariationDataOffsets.Length);

        stream.WriteUShortByBigEndian(Format);
        stream.WriteUIntByBigEndian(VariationRegionListRecord is null ? 0 : (uint)offset);
        stream.WriteUShortByBigEndian(ItemVariationDataCount);
        ItemVariationDataOffsets.Each(stream.WriteUIntByBigEndian);

        if (VariationRegionListRecord is { }) VariationRegionListRecord.WriteTo(stream);
    }

    public int SizeOf() => Format.SizeOf() + VariationRegionListOffset.SizeOf() + ItemVariationDataCount.SizeOf() + (sizeof(uint) * ItemVariationDataOffsets.Length) + (VariationRegionListRecord?.SizeOf() ?? 0);
}

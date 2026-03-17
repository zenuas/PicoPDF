using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables;

public class ColorPaletteTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfPaletteEntries { get; init; }
    public required ushort NumberOfPalettes { get; init; }
    public required ushort NumberOfColorRecords { get; init; }
    public required uint ColorRecordsArrayOffset { get; init; }
    public required ushort[] ColorRecordIndices { get; init; }
    public required uint PaletteTypesArrayOffset { get; init; }
    public required uint PaletteLabelsArrayOffset { get; init; }
    public required uint PaletteEntryLabelsArrayOffset { get; init; }
    public required ColorRecord[] ColorRecords { get; init; }
    public required uint[] PaletteTypes { get; init; }
    public required ushort[] PaletteLabels { get; init; }
    public required ushort[] PaletteEntryLabels { get; init; }

    public static ColorPaletteTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var version = stream.ReadUShortByBigEndian();
        var numPaletteEntries = stream.ReadUShortByBigEndian();
        var numPalettes = stream.ReadUShortByBigEndian();
        var numColorRecords = stream.ReadUShortByBigEndian();
        var colorRecordsArrayOffset = stream.ReadOffset32();
        var colorRecordIndices = Lists.Repeat(stream.ReadUShortByBigEndian).Take(numPalettes).ToArray();

        uint paletteTypesArrayOffset = 0;
        uint paletteLabelsArrayOffset = 0;
        uint paletteEntryLabelsArrayOffset = 0;
        if (version >= 1)
        {
            paletteTypesArrayOffset = stream.ReadOffset32();
            paletteLabelsArrayOffset = stream.ReadOffset32();
            paletteEntryLabelsArrayOffset = stream.ReadOffset32();
        }

        var colorRecords = stream.SeekTo(position + colorRecordsArrayOffset).To(_ => Lists.Repeat(() => ColorRecord.ReadFrom(stream)).Take(numColorRecords).ToArray());

        uint[] paletteTypes = paletteTypesArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteTypesArrayOffset).To(_ => Lists.Repeat(stream.ReadUIntByBigEndian).Take(numPalettes))];

        ushort[] paletteLabels = paletteLabelsArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteLabelsArrayOffset).To(_ => Lists.Repeat(stream.ReadUShortByBigEndian).Take(numPalettes))];

        ushort[] paletteEntryLabels = paletteEntryLabelsArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteEntryLabelsArrayOffset).To(_ => Lists.Repeat(stream.ReadUShortByBigEndian).Take(numPaletteEntries))];

        return new()
        {
            Version = version,
            NumberOfPaletteEntries = numPaletteEntries,
            NumberOfPalettes = numPalettes,
            NumberOfColorRecords = numColorRecords,
            ColorRecordsArrayOffset = colorRecordsArrayOffset,
            ColorRecordIndices = colorRecordIndices,
            PaletteTypesArrayOffset = paletteTypesArrayOffset,
            PaletteLabelsArrayOffset = paletteLabelsArrayOffset,
            PaletteEntryLabelsArrayOffset = paletteEntryLabelsArrayOffset,
            ColorRecords = colorRecords,
            PaletteTypes = paletteTypes,
            PaletteLabels = paletteLabels,
            PaletteEntryLabels = paletteEntryLabels,
        };
    }

    public void WriteTo(Stream stream)
    {
        var colorRecordsArrayOffset = Version.SizeOf() +
            NumberOfPaletteEntries.SizeOf() +
            NumberOfPalettes.SizeOf() +
            NumberOfColorRecords.SizeOf() +
            ColorRecordsArrayOffset.SizeOf() +
            (sizeof(ushort) * ColorRecordIndices.Length);

        stream.WriteUShortByBigEndian(Version);
        stream.WriteUShortByBigEndian((ushort)PaletteEntryLabels.Length);
        stream.WriteUShortByBigEndian((ushort)ColorRecordIndices.Length);
        stream.WriteUShortByBigEndian((ushort)ColorRecords.Length);
        stream.WriteOffset32((uint)colorRecordsArrayOffset);
        ColorRecordIndices.Each(stream.WriteUShortByBigEndian);

        if (Version == 0)
        {
            ColorRecords.Each(x => x.WriteTo(stream));
        }
        else
        {
            var sizeof_ColorRecords = ColorRecords.Select(x => x.SizeOf()).Sum();
            var sizeof_PaletteTypes = sizeof(uint) * PaletteTypes.Length;
            var sizeof_PaletteLabels = sizeof(ushort) * PaletteLabels.Length;

            var colorRecordsArrayOffsetV1 = colorRecordsArrayOffset +
                PaletteTypesArrayOffset.SizeOf() +
                PaletteLabelsArrayOffset.SizeOf() +
                PaletteEntryLabelsArrayOffset.SizeOf();

            stream.WriteOffset32((uint)(colorRecordsArrayOffsetV1 + sizeof_ColorRecords));
            stream.WriteOffset32((uint)(colorRecordsArrayOffsetV1 + sizeof_ColorRecords + sizeof_PaletteTypes));
            stream.WriteOffset32((uint)(colorRecordsArrayOffsetV1 + sizeof_ColorRecords + sizeof_PaletteTypes + sizeof_PaletteLabels));

            ColorRecords.Each(x => x.WriteTo(stream));
            PaletteTypes.Each(stream.WriteUIntByBigEndian);
            PaletteLabels.Each(stream.WriteUShortByBigEndian);
            PaletteEntryLabels.Each(stream.WriteUShortByBigEndian);
        }
    }
}

using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class ColorPaletteTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberPaletteEntries { get; init; }
    public required ushort NumberPalettes { get; init; }
    public required ushort NumberColorRecords { get; init; }
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
        var colorRecordsArrayOffset = stream.ReadUIntByBigEndian();
        var colorRecordIndices = Lists.Repeat(() => stream.ReadUShortByBigEndian())
                .Take(numPalettes)
                .ToArray();

        uint paletteTypesArrayOffset = 0;
        uint paletteLabelsArrayOffset = 0;
        uint paletteEntryLabelsArrayOffset = 0;

        if (version >= 1)
        {
            paletteTypesArrayOffset = stream.ReadUIntByBigEndian();
            paletteLabelsArrayOffset = stream.ReadUIntByBigEndian();
            paletteEntryLabelsArrayOffset = stream.ReadUIntByBigEndian();
        }

        stream.Position = position + colorRecordsArrayOffset;
        var colorRecords = Lists.Repeat(() => ColorRecord.ReadFrom(stream))
                .Take(numColorRecords)
                .ToArray();

        uint[] paletteTypes = paletteTypesArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteTypesArrayOffset).To(_ => Lists.Repeat(() => stream.ReadUIntByBigEndian()).Take(numPalettes))];

        ushort[] paletteLabels = paletteLabelsArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteLabelsArrayOffset).To(_ => Lists.Repeat(() => stream.ReadUShortByBigEndian()).Take(numPalettes))];

        ushort[] paletteEntryLabels = paletteEntryLabelsArrayOffset == 0 ? []
            : [.. stream.SeekTo(position + paletteEntryLabelsArrayOffset).To(_ => Lists.Repeat(() => stream.ReadUShortByBigEndian()).Take(numPaletteEntries))];

        return new()
        {
            Version = version,
            NumberPaletteEntries = numPaletteEntries,
            NumberPalettes = numPalettes,
            NumberColorRecords = numColorRecords,
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
        throw new NotImplementedException();
    }
}

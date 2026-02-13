using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class CMapTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required Dictionary<EncodingRecord, ICMapFormat> EncodingRecords { get; init; }

    public static CMapTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var ver = stream.ReadUShortByBigEndian();
        var num_of_tables = stream.ReadUShortByBigEndian();

        var offset_cmap = new Dictionary<uint, ICMapFormat>();

        return new()
        {
            Version = ver,
            NumberOfTables = num_of_tables,
            EncodingRecords = Enumerable.Repeat(0, num_of_tables)
                .Select(_ => EncodingRecord.ReadFrom(stream))
                .ToArray()
                .ToDictionary(x => x, x => offset_cmap.TryGetValue(x.Offset, out var cmap) ? cmap : (offset_cmap[x.Offset] = ReadCMapFormat(stream.SeekTo(position + x.Offset)))),
        };
    }

    public static ICMapFormat ReadCMapFormat(Stream stream)
    {
        var format = stream.ReadUShortByBigEndian();
        return format switch
        {
            0 => CMapFormat0.ReadFrom(stream),
            4 => CMapFormat4.ReadFrom(stream),
            12 => CMapFormat12.ReadFrom(stream),
            _ => new CMapFormatN { Format = format },
        };
    }

    public void WriteTo(Stream stream)
    {
        var export_cmap_keys = EncodingRecords
            .Where(x => x.Value is { })
            .Select(x => x.Key)
            .OrderBy(x => x.PlatformID)
            .ThenBy(x => x.EncodingID)
            .ToArray();

        var cmap_offset = /* sizeof(Version) + sizeof(NumberOfTables) */(sizeof(ushort) * 2) + (/* sizeof(EncodingRecord) */8 * export_cmap_keys.Length);
        stream.WriteUShortByBigEndian(Version);
        stream.WriteUShortByBigEndian((ushort)export_cmap_keys.Length);

        using var cmapformat = new MemoryStream();
        var cmap_offset_cache = new Dictionary<ICMapFormat, long>();

        export_cmap_keys
            .Select(x => (Key: x, Value: EncodingRecords[x]!))
            .Each(x =>
            {
                var offset = cmap_offset_cache.TryGetValue(x.Value, out var cache_offset) ? cache_offset : cmapformat.Position + cmap_offset;
                if (!cmap_offset_cache.ContainsKey(x.Value)) x.Value.WriteTo(cmapformat);
                cmap_offset_cache[x.Value] = offset;

                stream.WriteUShortByBigEndian(x.Key.PlatformID);
                stream.WriteUShortByBigEndian(x.Key.EncodingID);
                stream.WriteUIntByBigEndian((uint)offset);
            });

        stream.Write(cmapformat.ToArray());
    }

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

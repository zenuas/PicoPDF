using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class CMapTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required Dictionary<EncodingRecord, ICMapFormat?> EncodingRecords { get; init; }

    public static CMapTable ReadFrom(Stream stream)
    {
        var ver = stream.ReadUShortByBigEndian();
        var num_of_tables = stream.ReadUShortByBigEndian();

        return new()
        {
            Version = ver,
            NumberOfTables = num_of_tables,
            EncodingRecords = Enumerable.Range(0, num_of_tables).Select(_ => EncodingRecord.ReadFrom(stream)).ToDictionary(x => x, _ => (ICMapFormat?)null),
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

        var cmap_offset = (/* sizeof(Version) + sizeof(NumberOfTables) */sizeof(ushort) * 2) + (/* sizeof(EncodingRecord) */8 * export_cmap_keys.Length);
        stream.WriteUShortByBigEndian(Version);
        stream.WriteUShortByBigEndian((ushort)export_cmap_keys.Length);

        using var cmapformat = new MemoryStream();

        export_cmap_keys
            .Select(x => (Key: x, Value: EncodingRecords[x]!))
            .Each(x =>
            {
                var offset = cmapformat.Position + cmap_offset;
                x.Value.WriteTo(stream);

                stream.WriteUShortByBigEndian(x.Key.PlatformID);
                stream.WriteUShortByBigEndian(x.Key.EncodingID);
                stream.WriteUIntByBigEndian((uint)offset);
            });

        cmapformat.Position = 0;
        stream.Write(cmapformat.ToArray());
    }

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

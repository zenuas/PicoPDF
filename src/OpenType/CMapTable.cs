using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class CMapTable : IExportable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required EncodingRecord[] EncodingRecords { get; init; }

    public static CMapTable ReadFrom(Stream stream)
    {
        var ver = stream.ReadUShortByBigEndian();
        var num_of_tables = stream.ReadUShortByBigEndian();

        return new()
        {
            Version = ver,
            NumberOfTables = num_of_tables,
            EncodingRecords = Enumerable.Range(0, num_of_tables).Select(_ => EncodingRecord.ReadFrom(stream)).ToArray(),
        };
    }

    public long WriteTo(Stream stream)
    {
        var position = stream.Position;
        return stream.Position - position;
    }

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

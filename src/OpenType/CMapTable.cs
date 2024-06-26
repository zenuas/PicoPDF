using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class CMapTable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required EncodingRecord[] EncodingRecords { get; init; }

    public static CMapTable ReadFrom(Stream stream)
    {
        var ver = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var num_of_tables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        return new()
        {
            Version = ver,
            NumberOfTables = num_of_tables,
            EncodingRecords = Enumerable.Range(0, num_of_tables).Select(_ => EncodingRecord.ReadFrom(stream)).ToArray(),
        };
    }

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

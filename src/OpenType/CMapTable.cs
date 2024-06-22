using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class CMapTable
{
    public required ushort Version { get; init; }
    public required ushort NumberOfTables { get; init; }

    public static CMapTable ReadFrom(Stream stream) => new()
    {
        Version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
    };

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

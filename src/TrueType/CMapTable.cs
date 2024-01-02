using Mina.Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct CMapTable
{
    public ushort Version;
    public ushort NumberOfTables;

    public static CMapTable ReadFrom(Stream stream) => new()
    {
        Version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
    };

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

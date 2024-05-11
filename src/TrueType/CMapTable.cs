using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public class CMapTable(Stream stream)
{
    public readonly ushort Version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort NumberOfTables = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

    public override string ToString() => $"Version={Version}, NumberOfTables={NumberOfTables}";
}

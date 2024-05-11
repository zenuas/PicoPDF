using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public class NameTable(Stream stream)
{
    public readonly ushort Format = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort Count = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort StringOffset = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}

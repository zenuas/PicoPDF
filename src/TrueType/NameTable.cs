using Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct NameTable
{
    public ushort Format;
    public ushort Count;
    public ushort StringOffset;

    public static NameTable ReadFrom(Stream stream) => new()
    {
        Format = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Count = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        StringOffset = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))
    };

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}

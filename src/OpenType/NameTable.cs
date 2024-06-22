using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class NameTable
{
    public required ushort Format { get; init; }
    public required ushort Count { get; init; }
    public required ushort StringOffset { get; init; }

    public static NameTable ReadFrom(Stream stream) => new()
    {
        Format = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Count = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        StringOffset = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
    };

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}

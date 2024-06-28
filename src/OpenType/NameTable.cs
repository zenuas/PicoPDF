using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class NameTable
{
    public required ushort Format { get; init; }
    public required ushort Count { get; init; }
    public required ushort StringOffset { get; init; }

    public static NameTable ReadFrom(Stream stream) => new()
    {
        Format = stream.ReadUShortByBigEndian(),
        Count = stream.ReadUShortByBigEndian(),
        StringOffset = stream.ReadUShortByBigEndian(),
    };

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}

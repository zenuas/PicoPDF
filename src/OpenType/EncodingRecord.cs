using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class EncodingRecord
{
    public required ushort PlatformID { get; init; }
    public required ushort EncodingID { get; init; }
    public required uint Offset { get; init; }

    public PlatformEncodings PlatformEncoding { get => (PlatformEncodings)(((uint)PlatformID << 16) | EncodingID); }

    public static EncodingRecord ReadFrom(Stream stream) => new()
    {
        PlatformID = stream.ReadUShortByBigEndian(),
        EncodingID = stream.ReadUShortByBigEndian(),
        Offset = stream.ReadUIntByBigEndian(),
    };

    public override string ToString() => $"PlatformID={PlatformID}, EncodingID={EncodingID}";
}

using Mina.Extension;
using System.IO;
using System.Text;

namespace OpenType;

public class TrueTypeCollectionHeader
{
    public required string TTCTag { get; init; }
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required uint NumberOfFonts { get; init; }

    public static TrueTypeCollectionHeader ReadFrom(Stream stream) => new()
    {
        TTCTag = Encoding.ASCII.GetString(stream.ReadExactly(4)),
        MajorVersion = stream.ReadUShortByBigEndian(),
        MinorVersion = stream.ReadUShortByBigEndian(),
        NumberOfFonts = stream.ReadUIntByBigEndian(),
    };

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

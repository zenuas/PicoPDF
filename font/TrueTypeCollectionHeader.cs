using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType;

public class TrueTypeCollectionHeader
{
    public required string TTCTag { get; init; }
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required uint NumberOfFonts { get; init; }
    public required uint[] TableDirectoryOffsets { get; init; }
    public required uint DsigTag { get; init; }
    public required uint DsigLength { get; init; }
    public required uint DsigOffset { get; init; }

    public static TrueTypeCollectionHeader ReadFrom(Stream stream)
    {
        var ttcTag = Encoding.ASCII.GetString(stream.ReadExactly(4));
        var majorVersion = stream.ReadUShortByBigEndian();
        var minorVersion = stream.ReadUShortByBigEndian();
        var numFonts = stream.ReadUIntByBigEndian();
        var tableDirectoryOffsets = Lists.Repeat(stream.ReadOffset32).Take((int)numFonts).ToArray();

        var dsigTag = 0u;
        var dsigLength = 0u;
        var dsigOffset = 0u;
        if (majorVersion >= 2)
        {
            dsigTag = stream.ReadUIntByBigEndian();
            dsigLength = stream.ReadUIntByBigEndian();
            dsigOffset = stream.ReadUIntByBigEndian();
        }

        return new()
        {
            TTCTag = ttcTag,
            MajorVersion = majorVersion,
            MinorVersion = minorVersion,
            NumberOfFonts = numFonts,
            TableDirectoryOffsets = tableDirectoryOffsets,
            DsigTag = dsigTag,
            DsigLength = dsigLength,
            DsigOffset = dsigOffset,
        };
    }

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

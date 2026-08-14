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
    public required Offset32[] TableDirectoryOffsets { get; init; }
    public required uint DsigTag { get; init; }
    public required uint DsigLength { get; init; }
    public required uint DsigOffset { get; init; }

    public static TrueTypeCollectionHeader ReadFrom(Stream stream)
    {
        var ttc_tag = Encoding.ASCII.GetString(stream.ReadExactly(4));
        var major_version = stream.ReadUShortByBigEndian();
        var minor_version = stream.ReadUShortByBigEndian();
        var num_fonts = stream.ReadUIntByBigEndian();
        var table_directory_offsets = Lists.Repeat(stream.ReadOffset32).Take((int)num_fonts).ToArray();

        var dsig_tag = 0u;
        var dsig_length = 0u;
        var dsig_offset = 0u;
        if (major_version >= 2)
        {
            dsig_tag = stream.ReadUIntByBigEndian();
            dsig_length = stream.ReadUIntByBigEndian();
            dsig_offset = stream.ReadUIntByBigEndian();
        }

        return new()
        {
            TTCTag = ttc_tag,
            MajorVersion = major_version,
            MinorVersion = minor_version,
            NumberOfFonts = num_fonts,
            TableDirectoryOffsets = table_directory_offsets,
            DsigTag = dsig_tag,
            DsigLength = dsig_length,
            DsigOffset = dsig_offset,
        };
    }

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

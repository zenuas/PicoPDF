using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType;

public class TrueTypeCollectionHeader
{
    public required string TTCTag { get; init; }
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required uint NumberOfFonts { get; init; }

    public static TrueTypeCollectionHeader ReadFrom(Stream stream) => new()
    {
        TTCTag = Encoding.ASCII.GetString(stream.ReadBytes(4)),
        MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NumberOfFonts = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
    };

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

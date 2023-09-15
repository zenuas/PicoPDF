using Extensions;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.TrueType;

public struct TrueTypeCollectionHeader
{
    public string TTCTag;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint NumberOfFonts;

    public static TrueTypeCollectionHeader ReadFrom(Stream stream) => new()
    {
        TTCTag = Encoding.ASCII.GetString(stream.ReadBytes(4)),
        MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NumberOfFonts = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4))
    };

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

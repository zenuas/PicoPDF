using Extensions;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.Document.Font.TrueType;

public struct TTCHeader
{
    public string TTCTag;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint NumberOfFonts;

    public static TTCHeader ReadFrom(Stream stream) => new()
    {
        TTCTag = Encoding.ASCII.GetString(stream.ReadBytes(4)),
        MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        NumberOfFonts = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4))
    };

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

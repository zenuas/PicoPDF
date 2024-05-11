using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.TrueType;

public class TrueTypeCollectionHeader(Stream stream)
{
    public readonly string TTCTag = Encoding.ASCII.GetString(stream.ReadBytes(4));
    public readonly ushort MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly uint NumberOfFonts = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));

    public override string ToString() => $"{TTCTag}, MajorVersion={MajorVersion}, MinorVersion={MinorVersion}";
}

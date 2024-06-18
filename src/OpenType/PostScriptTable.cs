using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class PostScriptTable(Stream stream)
{
    public readonly uint Version = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly int ItalicAngle = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4));
    public readonly short UnderlinePosition = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short UnderlineThickness = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly uint IsFixedPitch = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint MinMemType42 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint MaxMemType42 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint MinMemType1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint MaxMemType1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
}

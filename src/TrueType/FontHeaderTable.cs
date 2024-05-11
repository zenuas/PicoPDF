using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public class FontHeaderTable(Stream stream)
{
    public readonly ushort MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly int FontRevision = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4));
    public readonly uint ChecksumAdjustment = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint MagicNumber = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly ushort Flags = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort UnitsPerEm = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly long Created = BinaryPrimitives.ReadInt64BigEndian(stream.ReadBytes(8));
    public readonly long Modified = BinaryPrimitives.ReadInt64BigEndian(stream.ReadBytes(8));
    public readonly short XMin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short YMin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short XMax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short YMax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort MacStyle = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly ushort LowestRecPPEM = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly short FontDirectionHint = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short IndexToLocFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
    public readonly short GlyphDataFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
}

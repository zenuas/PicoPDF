using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct FontHeaderTable
{
    public ushort MajorVersion;
    public ushort MinorVersion;
    public int FontRevision;
    public uint ChecksumAdjustment;
    public uint MagicNumber;
    public ushort Flags;
    public ushort UnitsPerEm;
    public long Created;
    public long Modified;
    public short XMin;
    public short YMin;
    public short XMax;
    public short YMax;
    public ushort MacStyle;
    public ushort LowestRecPPEM;
    public short FontDirectionHint;
    public short IndexToLocFormat;
    public short GlyphDataFormat;

    public static FontHeaderTable ReadFrom(Stream stream) => new()
    {
        MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        FontRevision = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4)),
        ChecksumAdjustment = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        MagicNumber = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        Flags = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        UnitsPerEm = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        Created = BinaryPrimitives.ReadInt64BigEndian(stream.ReadBytes(8)),
        Modified = BinaryPrimitives.ReadInt64BigEndian(stream.ReadBytes(8)),
        XMin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        YMin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        XMax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        YMax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        MacStyle = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        LowestRecPPEM = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        FontDirectionHint = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        IndexToLocFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        GlyphDataFormat = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
    };
}

using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class FontHeaderTable
{
    public required ushort MajorVersion { get; init; }
    public required ushort MinorVersion { get; init; }
    public required int FontRevision { get; init; }
    public required uint ChecksumAdjustment { get; init; }
    public required uint MagicNumber { get; init; }
    public required ushort Flags { get; init; }
    public required ushort UnitsPerEm { get; init; }
    public required long Created { get; init; }
    public required long Modified { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required ushort MacStyle { get; init; }
    public required ushort LowestRecPPEM { get; init; }
    public required short FontDirectionHint { get; init; }
    public required short IndexToLocFormat { get; init; }
    public required short GlyphDataFormat { get; init; }

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

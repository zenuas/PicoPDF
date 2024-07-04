using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class FontHeaderTable : IExportable
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
        MajorVersion = stream.ReadUShortByBigEndian(),
        MinorVersion = stream.ReadUShortByBigEndian(),
        FontRevision = stream.ReadIntByBigEndian(),
        ChecksumAdjustment = stream.ReadUIntByBigEndian(),
        MagicNumber = stream.ReadUIntByBigEndian(),
        Flags = stream.ReadUShortByBigEndian(),
        UnitsPerEm = stream.ReadUShortByBigEndian(),
        Created = stream.ReadLongByBigEndian(),
        Modified = stream.ReadLongByBigEndian(),
        XMin = stream.ReadShortByBigEndian(),
        YMin = stream.ReadShortByBigEndian(),
        XMax = stream.ReadShortByBigEndian(),
        YMax = stream.ReadShortByBigEndian(),
        MacStyle = stream.ReadUShortByBigEndian(),
        LowestRecPPEM = stream.ReadUShortByBigEndian(),
        FontDirectionHint = stream.ReadShortByBigEndian(),
        IndexToLocFormat = stream.ReadShortByBigEndian(),
        GlyphDataFormat = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(MajorVersion);
        stream.WriteUShortByBigEndian(MinorVersion);
        stream.WriteIntByBigEndian(FontRevision);
        stream.WriteUIntByBigEndian(ChecksumAdjustment);
        stream.WriteUIntByBigEndian(MagicNumber);
        stream.WriteUShortByBigEndian(Flags);
        stream.WriteUShortByBigEndian(UnitsPerEm);
        stream.WriteLongByBigEndian(Created);
        stream.WriteLongByBigEndian(Modified);
        stream.WriteShortByBigEndian(XMin);
        stream.WriteShortByBigEndian(YMin);
        stream.WriteShortByBigEndian(XMax);
        stream.WriteShortByBigEndian(YMax);
        stream.WriteUShortByBigEndian(MacStyle);
        stream.WriteUShortByBigEndian(LowestRecPPEM);
        stream.WriteShortByBigEndian(FontDirectionHint);
        stream.WriteShortByBigEndian(IndexToLocFormat);
        stream.WriteShortByBigEndian(GlyphDataFormat);
    }
}

using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType;

public class OS2Table
{
    public required ushort Version { get; init; }
    public required short XAvgCharWidth { get; init; }
    public required ushort UsWeightClass { get; init; }
    public required ushort UsWidthClass { get; init; }
    public required ushort FsType { get; init; }
    public required short YSubscriptXSize { get; init; }
    public required short YSubscriptYSize { get; init; }
    public required short YSubscriptXOffset { get; init; }
    public required short YSubscriptYOffset { get; init; }
    public required short YSuperscriptXSize { get; init; }
    public required short YSuperscriptYSize { get; init; }
    public required short YSuperscriptXOffset { get; init; }
    public required short YSuperscriptYOffset { get; init; }
    public required short YStrikeoutSize { get; init; }
    public required short YStrikeoutPosition { get; init; }
    public required short SFamilyClass { get; init; }
    public required byte[] Panose { get; init; }
    public required uint UlUnicodeRange1 { get; init; }
    public required uint UlUnicodeRange2 { get; init; }
    public required uint UlUnicodeRange3 { get; init; }
    public required uint UlUnicodeRange4 { get; init; }
    public required string AchVendID { get; init; }
    public required ushort FsSelection { get; init; }
    public required ushort UsFirstCharIndex { get; init; }
    public required ushort UsLastCharIndex { get; init; }
    public required short STypoAscender { get; init; }
    public required short STypoDescender { get; init; }
    public required short STypoLineGap { get; init; }
    public required ushort UsWinAscent { get; init; }
    public required ushort UsWinDescent { get; init; }

    public uint UlCodePageRange1 { get; init; }
    public uint UlCodePageRange2 { get; init; }

    public short SxHeight { get; init; }
    public short SCapHeight { get; init; }
    public ushort UsDefaultChar { get; init; }
    public ushort UsBreakChar { get; init; }
    public ushort UsMaxContext { get; init; }

    public ushort UsLowerOpticalPointSize { get; init; }
    public ushort UsUpperOpticalPointSize { get; init; }

    public static OS2Table ReadFrom(Stream stream)
    {
        var version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        return new()
        {
            Version = version,
            XAvgCharWidth = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            UsWeightClass = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            UsWidthClass = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            FsType = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            YSubscriptXSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSubscriptYSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSubscriptXOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSubscriptYOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSuperscriptXSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSuperscriptYSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSuperscriptXOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YSuperscriptYOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YStrikeoutSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            YStrikeoutPosition = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            SFamilyClass = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            Panose = stream.ReadBytes(10),
            UlUnicodeRange1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
            UlUnicodeRange2 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
            UlUnicodeRange3 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
            UlUnicodeRange4 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
            AchVendID = Encoding.ASCII.GetString(stream.ReadBytes(4)),
            FsSelection = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            UsFirstCharIndex = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            UsLastCharIndex = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            STypoAscender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            STypoDescender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            STypoLineGap = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
            UsWinAscent = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            UsWinDescent = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            UlCodePageRange1 = version >= 1 ? BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)) : 0,
            UlCodePageRange2 = version >= 1 ? BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)) : 0,
            SxHeight = version >= 2 ? BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)) : (short)0,
            SCapHeight = version >= 2 ? BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)) : (short)0,
            UsDefaultChar = version >= 2 ? BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)) : (ushort)0,
            UsBreakChar = version >= 2 ? BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)) : (ushort)0,
            UsMaxContext = version >= 2 ? BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)) : (ushort)0,
            UsLowerOpticalPointSize = version >= 5 ? BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)) : (ushort)0,
            UsUpperOpticalPointSize = version >= 5 ? BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)) : (ushort)0,
        };
    }
}

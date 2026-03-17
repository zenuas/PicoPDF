using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class OS2Table : IExportable
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
        var version = stream.ReadUShortByBigEndian();
        return new()
        {
            Version = version,
            XAvgCharWidth = stream.ReadFWORD(),
            UsWeightClass = stream.ReadUShortByBigEndian(),
            UsWidthClass = stream.ReadUShortByBigEndian(),
            FsType = stream.ReadUShortByBigEndian(),
            YSubscriptXSize = stream.ReadFWORD(),
            YSubscriptYSize = stream.ReadFWORD(),
            YSubscriptXOffset = stream.ReadFWORD(),
            YSubscriptYOffset = stream.ReadFWORD(),
            YSuperscriptXSize = stream.ReadFWORD(),
            YSuperscriptYSize = stream.ReadFWORD(),
            YSuperscriptXOffset = stream.ReadFWORD(),
            YSuperscriptYOffset = stream.ReadFWORD(),
            YStrikeoutSize = stream.ReadFWORD(),
            YStrikeoutPosition = stream.ReadFWORD(),
            SFamilyClass = stream.ReadShortByBigEndian(),
            Panose = stream.ReadExactly(10),
            UlUnicodeRange1 = stream.ReadUIntByBigEndian(),
            UlUnicodeRange2 = stream.ReadUIntByBigEndian(),
            UlUnicodeRange3 = stream.ReadUIntByBigEndian(),
            UlUnicodeRange4 = stream.ReadUIntByBigEndian(),
            AchVendID = stream.ReadTag(),
            FsSelection = stream.ReadUShortByBigEndian(),
            UsFirstCharIndex = stream.ReadUShortByBigEndian(),
            UsLastCharIndex = stream.ReadUShortByBigEndian(),
            STypoAscender = stream.ReadFWORD(),
            STypoDescender = stream.ReadFWORD(),
            STypoLineGap = stream.ReadFWORD(),
            UsWinAscent = stream.ReadUFWORD(),
            UsWinDescent = stream.ReadUFWORD(),
            UlCodePageRange1 = version >= 1 ? stream.ReadUIntByBigEndian() : 0,
            UlCodePageRange2 = version >= 1 ? stream.ReadUIntByBigEndian() : 0,
            SxHeight = version >= 2 ? stream.ReadFWORD() : (short)0,
            SCapHeight = version >= 2 ? stream.ReadFWORD() : (short)0,
            UsDefaultChar = version >= 2 ? stream.ReadUShortByBigEndian() : (ushort)0,
            UsBreakChar = version >= 2 ? stream.ReadUShortByBigEndian() : (ushort)0,
            UsMaxContext = version >= 2 ? stream.ReadUShortByBigEndian() : (ushort)0,
            UsLowerOpticalPointSize = version >= 5 ? stream.ReadUShortByBigEndian() : (ushort)0,
            UsUpperOpticalPointSize = version >= 5 ? stream.ReadUShortByBigEndian() : (ushort)0,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Version);
        stream.WriteFWORD(XAvgCharWidth);
        stream.WriteUShortByBigEndian(UsWeightClass);
        stream.WriteUShortByBigEndian(UsWidthClass);
        stream.WriteUShortByBigEndian(FsType);
        stream.WriteFWORD(YSubscriptXSize);
        stream.WriteFWORD(YSubscriptYSize);
        stream.WriteFWORD(YSubscriptXOffset);
        stream.WriteFWORD(YSubscriptYOffset);
        stream.WriteFWORD(YSuperscriptXSize);
        stream.WriteFWORD(YSuperscriptYSize);
        stream.WriteFWORD(YSuperscriptXOffset);
        stream.WriteFWORD(YSuperscriptYOffset);
        stream.WriteFWORD(YStrikeoutSize);
        stream.WriteFWORD(YStrikeoutPosition);
        stream.WriteShortByBigEndian(SFamilyClass);
        stream.Write(Panose);
        stream.WriteUIntByBigEndian(UlUnicodeRange1);
        stream.WriteUIntByBigEndian(UlUnicodeRange2);
        stream.WriteUIntByBigEndian(UlUnicodeRange3);
        stream.WriteUIntByBigEndian(UlUnicodeRange4);
        stream.WriteTag(AchVendID);
        stream.WriteUShortByBigEndian(FsSelection);
        stream.WriteUShortByBigEndian(UsFirstCharIndex);
        stream.WriteUShortByBigEndian(UsLastCharIndex);
        stream.WriteFWORD(STypoAscender);
        stream.WriteFWORD(STypoDescender);
        stream.WriteFWORD(STypoLineGap);
        stream.WriteUFWORD(UsWinAscent);
        stream.WriteUFWORD(UsWinDescent);

        if (Version >= 1)
        {
            stream.WriteUIntByBigEndian(UlCodePageRange1);
            stream.WriteUIntByBigEndian(UlCodePageRange2);
        }
        if (Version >= 2)
        {
            stream.WriteFWORD(SxHeight);
            stream.WriteFWORD(SCapHeight);
            stream.WriteUShortByBigEndian(UsDefaultChar);
            stream.WriteUShortByBigEndian(UsBreakChar);
            stream.WriteUShortByBigEndian(UsMaxContext);
        }
        if (Version >= 5)
        {
            stream.WriteUShortByBigEndian(UsLowerOpticalPointSize);
            stream.WriteUShortByBigEndian(UsUpperOpticalPointSize);
        }
    }
}

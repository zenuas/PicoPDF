using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public record class OS2Table : IExportable
{
    public required ushort Version { get; init; }
    public required FWORD XAvgCharWidth { get; init; }
    public required ushort UsWeightClass { get; init; }
    public required ushort UsWidthClass { get; init; }
    public required ushort FsType { get; init; }
    public required FWORD YSubscriptXSize { get; init; }
    public required FWORD YSubscriptYSize { get; init; }
    public required FWORD YSubscriptXOffset { get; init; }
    public required FWORD YSubscriptYOffset { get; init; }
    public required FWORD YSuperscriptXSize { get; init; }
    public required FWORD YSuperscriptYSize { get; init; }
    public required FWORD YSuperscriptXOffset { get; init; }
    public required FWORD YSuperscriptYOffset { get; init; }
    public required FWORD YStrikeoutSize { get; init; }
    public required FWORD YStrikeoutPosition { get; init; }
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
    public required FWORD STypoAscender { get; init; }
    public required FWORD STypoDescender { get; init; }
    public required FWORD STypoLineGap { get; init; }
    public required UFWORD UsWinAscent { get; init; }
    public required UFWORD UsWinDescent { get; init; }

    public uint UlCodePageRange1 { get; init; }
    public uint UlCodePageRange2 { get; init; }

    public FWORD SxHeight { get; init; }
    public FWORD SCapHeight { get; init; }
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

    public static OS2Table Create() => new()
    {
        Version = 5,
        XAvgCharWidth = 0,
        UsWeightClass = 0,
        UsWidthClass = 0,
        FsType = 0,
        YSubscriptXSize = 0,
        YSubscriptYSize = 0,
        YSubscriptXOffset = 0,
        YSubscriptYOffset = 0,
        YSuperscriptXSize = 0,
        YSuperscriptYSize = 0,
        YSuperscriptXOffset = 0,
        YSuperscriptYOffset = 0,
        YStrikeoutSize = 0,
        YStrikeoutPosition = 0,
        SFamilyClass = 0,
        Panose = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        UlUnicodeRange1 = 0,
        UlUnicodeRange2 = 0,
        UlUnicodeRange3 = 0,
        UlUnicodeRange4 = 0,
        AchVendID = "    ",
        FsSelection = 0,
        UsFirstCharIndex = 0,
        UsLastCharIndex = 0,
        STypoAscender = 0,
        STypoDescender = 0,
        STypoLineGap = 0,
        UsWinAscent = 0,
        UsWinDescent = 0,
        UlCodePageRange1 = 0,
        UlCodePageRange2 = 0,
        SxHeight = 0,
        SCapHeight = 0,
        UsDefaultChar = 0,
        UsBreakChar = 0,
        UsMaxContext = 0,
        UsLowerOpticalPointSize = 0,
        UsUpperOpticalPointSize = 0,
    };
}

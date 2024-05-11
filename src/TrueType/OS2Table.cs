using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.TrueType;

public class OS2Table
{
    public readonly ushort Version;
    public readonly short XAvgCharWidth;
    public readonly ushort UsWeightClass;
    public readonly ushort UsWidthClass;
    public readonly ushort FsType;
    public readonly short YSubscriptXSize;
    public readonly short YSubscriptYSize;
    public readonly short YSubscriptXOffset;
    public readonly short YSubscriptYOffset;
    public readonly short YSuperscriptXSize;
    public readonly short YSuperscriptYSize;
    public readonly short YSuperscriptXOffset;
    public readonly short YSuperscriptYOffset;
    public readonly short YStrikeoutSize;
    public readonly short YStrikeoutPosition;
    public readonly short SFamilyClass;
    public readonly byte[] Panose;
    public readonly uint UlUnicodeRange1;
    public readonly uint UlUnicodeRange2;
    public readonly uint UlUnicodeRange3;
    public readonly uint UlUnicodeRange4;
    public readonly string AchVendID;
    public readonly ushort FsSelection;
    public readonly ushort UsFirstCharIndex;
    public readonly ushort UsLastCharIndex;
    public readonly short STypoAscender;
    public readonly short STypoDescender;
    public readonly short STypoLineGap;
    public readonly ushort UsWinAscent;
    public readonly ushort UsWinDescent;

    public readonly uint UlCodePageRange1;
    public readonly uint UlCodePageRange2;

    public readonly short SxHeight;
    public readonly short SCapHeight;
    public readonly ushort UsDefaultChar;
    public readonly ushort UsBreakChar;
    public readonly ushort UsMaxContext;

    public readonly ushort UsLowerOpticalPointSize;
    public readonly ushort UsUpperOpticalPointSize;

    public OS2Table(Stream stream)
    {
        Version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        XAvgCharWidth = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        UsWeightClass = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        UsWidthClass = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        FsType = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        YSubscriptXSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSubscriptYSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSubscriptXOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSubscriptYOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSuperscriptXSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSuperscriptYSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSuperscriptXOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YSuperscriptYOffset = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YStrikeoutSize = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        YStrikeoutPosition = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        SFamilyClass = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        Panose = stream.ReadBytes(10);
        UlUnicodeRange1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        UlUnicodeRange2 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        UlUnicodeRange3 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        UlUnicodeRange4 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        AchVendID = Encoding.ASCII.GetString(stream.ReadBytes(4));
        FsSelection = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        UsFirstCharIndex = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        UsLastCharIndex = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        STypoAscender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        STypoDescender = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        STypoLineGap = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        UsWinAscent = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        UsWinDescent = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        if (Version >= 1)
        {
            UlCodePageRange1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
            UlCodePageRange2 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        }

        if (Version >= 2)
        {
            SxHeight = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            SCapHeight = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            UsDefaultChar = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            UsBreakChar = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            UsMaxContext = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }

        if (Version >= 5)
        {
            UsLowerOpticalPointSize = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            UsUpperOpticalPointSize = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }
    }
}

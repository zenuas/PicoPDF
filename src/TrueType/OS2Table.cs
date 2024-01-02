using Mina.Extensions;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.TrueType;

public struct OS2Table
{
    public ushort Version;
    public short XAvgCharWidth;
    public ushort UsWeightClass;
    public ushort UsWidthClass;
    public ushort FsType;
    public short YSubscriptXSize;
    public short YSubscriptYSize;
    public short YSubscriptXOffset;
    public short YSubscriptYOffset;
    public short YSuperscriptXSize;
    public short YSuperscriptYSize;
    public short YSuperscriptXOffset;
    public short YSuperscriptYOffset;
    public short YStrikeoutSize;
    public short YStrikeoutPosition;
    public short SFamilyClass;
    public byte[] Panose;
    public uint UlUnicodeRange1;
    public uint UlUnicodeRange2;
    public uint UlUnicodeRange3;
    public uint UlUnicodeRange4;
    public string AchVendID;
    public ushort FsSelection;
    public ushort UsFirstCharIndex;
    public ushort UsLastCharIndex;
    public short STypoAscender;
    public short STypoDescender;
    public short STypoLineGap;
    public ushort UsWinAscent;
    public ushort UsWinDescent;

    public uint UlCodePageRange1;
    public uint UlCodePageRange2;

    public short SxHeight;
    public short SCapHeight;
    public ushort UsDefaultChar;
    public ushort UsBreakChar;
    public ushort UsMaxContext;

    public ushort UsLowerOpticalPointSize;
    public ushort UsUpperOpticalPointSize;

    public static OS2Table ReadFrom(Stream stream)
    {
        var os2 = new OS2Table()
        {
            Version = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
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
        };

        if (os2.Version >= 1)
        {
            os2.UlCodePageRange1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
            os2.UlCodePageRange2 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        }

        if (os2.Version >= 2)
        {
            os2.SxHeight = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            os2.SCapHeight = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            os2.UsDefaultChar = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            os2.UsBreakChar = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            os2.UsMaxContext = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }

        if (os2.Version >= 5)
        {
            os2.UsLowerOpticalPointSize = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            os2.UsUpperOpticalPointSize = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }
        return os2;
    }
}

using System;

namespace PicoPDF.OpenType;

public enum PlatformEncodings
{
    [Obsolete("deprecated")]
    UnicodeUnicode1_0 = (0 << 16) | 0,

    [Obsolete("deprecated")]
    UnicodeUnicode1_1 = (0 << 16) | 1,

    [Obsolete("deprecated")]
    UnicodeISO_IEC_10646 = (0 << 16) | 2,

    UnicodeBMPOnly = (0 << 16) | 3,

    UnicodeFullRepertoire = (0 << 16) | 4,

    UnicodeFormat14 = (0 << 16) | 5,

    UnicodeFullRepertoireFormat13 = (0 << 16) | 6,

    [Obsolete("deprecated")]
    Macintosh = (1 << 16) | 0,

    ISO_7bitASCII = (2 << 16) | 0,

    ISO_ISO10646 = (2 << 16) | 1,

    ISO_ISO8859_1 = (2 << 16) | 2,

    Windows_Symbol = (3 << 16) | 0,

    Windows_UnicodeBMP = (3 << 16) | 1,

    Windows_ShiftJIS = (3 << 16) | 2,

    Windows_PRC = (3 << 16) | 3,

    Windows_Big5 = (3 << 16) | 4,

    Windows_Wansung = (3 << 16) | 5,

    Windows_Johab = (3 << 16) | 6,

    Windows_Reserved1 = (3 << 16) | 7,

    Windows_Reserved2 = (3 << 16) | 8,

    Windows_Reserved3 = (3 << 16) | 9,

    Windows_UnicodeFullRepertoire = (3 << 16) | 10,

    [Obsolete("deprecated")]
    Custom = (4 << 16) | 0,
}

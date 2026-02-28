using System;

namespace OpenType;

public enum PlatformEncodings
{
    [Obsolete("deprecated")]
    Unicode1_0 = (Platforms.Unicode << 16) | Encodings.Unicode1_0,

    [Obsolete("deprecated")]
    Unicode1_1 = (Platforms.Unicode << 16) | Encodings.Unicode1_1,

    [Obsolete("deprecated")]
    ISO_IEC_10646 = (Platforms.Unicode << 16) | Encodings.ISO_IEC_10646,

    UnicodeBMPOnly = (Platforms.Unicode << 16) | Encodings.Unicode2_0_BMPOnly,

    Unicode2_0_FullRepertoire = (Platforms.Unicode << 16) | Encodings.Unicode2_0_FullRepertoire,

    UnicodeVariationSequences = (Platforms.Unicode << 16) | Encodings.UnicodeVariationSequences,

    UnicodeFullRepertoire = (Platforms.Unicode << 16) | Encodings.UnicodeFullRepertoire,

    [Obsolete("deprecated")]
    Macintosh = (Platforms.Macintosh << 16) | 0,

    [Obsolete("deprecated")]
    ASCII7bit = (Platforms.ISO << 16) | Encodings.ASCII7bit,

    [Obsolete("deprecated")]
    ISO10646 = (Platforms.ISO << 16) | Encodings.ISO10646,

    [Obsolete("deprecated")]
    ISO8859_1 = (Platforms.ISO << 16) | Encodings.ISO8859_1,

    Windows_Symbol = (Platforms.Windows << 16) | Encodings.Windows_Symbol,

    Windows_UnicodeBMP = (Platforms.Windows << 16) | Encodings.Windows_UnicodeBMP,

    Windows_ShiftJIS = (Platforms.Windows << 16) | Encodings.Windows_ShiftJIS,

    Windows_PRC = (Platforms.Windows << 16) | Encodings.Windows_PRC,

    Windows_Big5 = (Platforms.Windows << 16) | Encodings.Windows_Big5,

    Windows_Wansung = (Platforms.Windows << 16) | Encodings.Windows_Wansung,

    Windows_Johab = (Platforms.Windows << 16) | Encodings.Windows_Johab,

    Windows_Reserved1 = (Platforms.Windows << 16) | Encodings.Windows_Reserved1,

    Windows_Reserved2 = (Platforms.Windows << 16) | Encodings.Windows_Reserved2,

    Windows_Reserved3 = (Platforms.Windows << 16) | Encodings.Windows_Reserved3,

    Windows_UnicodeFullRepertoire = (Platforms.Windows << 16) | Encodings.Windows_UnicodeFullRepertoire,

    [Obsolete("deprecated")]
    Custom = (Platforms.Custom << 16) | 0,
}

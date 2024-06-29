using System;

namespace PicoPDF.OpenType;

#pragma warning disable CA1069 // Enums values should not be duplicated

public enum Encodings
{
    [Obsolete("deprecated")]
    Unicode1_0 = 0,

    [Obsolete("deprecated")]
    Unicode1_1 = 1,

    [Obsolete("deprecated")]
    ISO_IEC_10646 = 2,

    Unicode2_0_BMPOnly = 3,

    Unicode2_0_FullRepertoire = 4,

    UnicodeVariationSequences = 5,

    UnicodeFullRepertoire = 6,

    ASCII7bit = 0,

    ISO10646 = 1,

    ISO8859_1 = 2,

    Windows_Symbol = 0,

    Windows_UnicodeBMP = 1,

    Windows_ShiftJIS = 2,

    Windows_PRC = 3,

    Windows_Big5 = 4,

    Windows_Wansung = 5,

    Windows_Johab = 6,

    Windows_Reserved1 = 7,

    Windows_Reserved2 = 8,

    Windows_Reserved3 = 9,

    Windows_UnicodeFullRepertoire = 10,
}

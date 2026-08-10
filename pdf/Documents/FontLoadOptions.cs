using System;

namespace Pdf.Documents;

[Flags]
public enum FontLoadOptions
{
    /// <summary>
    /// font emmed, but Restricted License font is not embed.
    /// </summary>
    PossibleEmbed = 0,
    ForceEmbed = 1,
    NotEmbed = 2,
    Stroke = 3,

    ConvertNone = 0 << 4,
    ConvertToTrueType = 1 << 4,
    ConvertToPostScript = 2 << 4,

    AlignHorizontal = 0 << 8,
    AlignVertical = 1 << 8,

    EmbedsMask = 0x000F,
    ConvertMask = 0x00F0,
    AlignMask = 0x0F00,
}

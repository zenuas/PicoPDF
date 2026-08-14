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

    HorizontalLeftToRight = 1 << 8,
    HorizontalRightToLeft = 2 << 8,
    VerticalLeftToRight = 4 << 8,
    VerticalRightToLeft = 8 << 8,

    Monospace = 1 << 12,
    Proportional = 2 << 12,

    EmbedsMask = 0x000F,
    ConvertMask = 0x00F0,
    AlignMask = 0x0F00,
    HorizontalMask = HorizontalLeftToRight | HorizontalRightToLeft,
    VerticalMask = VerticalLeftToRight | VerticalRightToLeft,
    SpacingMask = 0xF000,
}

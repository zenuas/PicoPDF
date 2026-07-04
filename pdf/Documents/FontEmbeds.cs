using System;

namespace Pdf.Documents;

[Flags]
public enum FontEmbeds
{
    NotEmbed = 1,
    /// <summary>
    /// font emmed, but Restricted License font is not embed.
    /// </summary>
    PossibleEmbed = 2,
    ForceEmbed = 3,
    Stroke = 4,

    ConvertNone = 0 << 8,
    ConvertToTrueType = 1 << 8,
    ConvertToPostScript = 2 << 8,

    EmbedsMask = 0x00FF,
    ConvertMask = 0xFF00,
}

using System;

namespace Pdf.Documents;

[Flags]
public enum FontEmbeds
{
    /// <summary>
    /// font emmed, but Restricted License font is not embed.
    /// </summary>
    PossibleEmbed = 0,
    ForceEmbed = 1,
    NotEmbed = 2,
    Stroke = 3,

    ConvertNone = 0 << 8,
    ConvertToTrueType = 1 << 8,
    ConvertToPostScript = 2 << 8,

    EmbedsMask = 0x00FF,
    ConvertMask = 0xFF00,
}

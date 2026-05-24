namespace PicoPDF.Loader.Sections;

public enum FontEmbeds
{
    NotEmbed,

    /// <summary>
    /// font emmed, but Restricted License font is not embed.
    /// </summary>
    PossibleEmbed,

    ForceEmbed,

    Stroke,
}

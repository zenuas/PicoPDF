using Pdf.Documents;

namespace PicoPDF.Loader.Sections;

public class FontPath
{
    public required string Path { get; init; }
    public FontEmbeds Embed { get; init; } = FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone;
}

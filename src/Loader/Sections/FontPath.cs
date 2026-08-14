using Pdf.Documents;

namespace PicoPDF.Loader.Sections;

public class FontPath
{
    public required string Path { get; init; }
    public FontLoadOptions Option { get; init; } = FontLoadOptions.PossibleEmbed | FontLoadOptions.ConvertNone | FontLoadOptions.HorizontalLeftToRight | FontLoadOptions.Monospace;
}

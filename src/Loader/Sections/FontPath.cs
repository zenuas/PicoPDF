namespace PicoPDF.Loader.Sections;

public class FontPath
{
    public required string Path { get; init; }
    public FontEmbed Embed { get; init; } = FontEmbed.PossibleEmbed;
}

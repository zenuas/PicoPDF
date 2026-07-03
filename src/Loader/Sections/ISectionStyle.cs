namespace PicoPDF.Loader.Sections;

public interface ISectionStyle
{
    public bool IsHeightAdjusting { get; }
    public SectionStyles Style { get; init; }
}

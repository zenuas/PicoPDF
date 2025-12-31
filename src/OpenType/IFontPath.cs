namespace PicoPDF.OpenType;

public interface IFontPath
{
    public string Path { get; init; }
    public bool ForceEmbedded { get; init; }
}

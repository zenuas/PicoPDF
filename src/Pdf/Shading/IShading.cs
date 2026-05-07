namespace PicoPDF.Pdf.Shading;

public interface IShading
{
    public string Name { get; init; }
    public ShadingTypes ShadingType { get; init; }
    public string ColorSpace { get; init; }
}

namespace PicoPDF.Pdf.Shading;

public interface IShading
{
    public ShadingTypes ShadingType { get; init; }
    public string ColorSpace { get; init; }
}

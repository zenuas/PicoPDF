namespace PicoPDF.Model.Elements;

public interface IImageModel : IModelElement
{
    public string Path { get; init; }
    public double ZoomWidth { get; init; }
    public double ZoomHeight { get; init; }
}

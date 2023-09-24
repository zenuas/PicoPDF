namespace PicoPDF.Binder.Element;

public class ImageElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Path { get; init; }
    public required string Bind { get; init; }
    public double ZoomWidth { get; init; } = 1.0;
    public double ZoomHeight { get; init; } = 1.0;
}

namespace PicoPDF.Model.Element;

public class ImageModel : IModelElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Path { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public double ZoomWidth { get; init; } = 1.0;
    public double ZoomHeight { get; init; } = 1.0;

    public override string ToString() => $"{Path}, X={X}, Y={Y}";
}

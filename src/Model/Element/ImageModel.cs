namespace PicoPDF.Model.Element;

public class ImageModel : IModelElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Path { get; init; }

    public override string ToString() => $"{Path}, X={X}, Y={Y}";
}

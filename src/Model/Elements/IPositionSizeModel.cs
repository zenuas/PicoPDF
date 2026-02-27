namespace PicoPDF.Model.Elements;

public interface IPositionSizeModel : IModelElement
{
    public int Width { get; init; }
    public int Height { get; }
}

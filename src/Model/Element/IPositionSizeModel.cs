namespace PicoPDF.Model.Element;

public interface IPositionSizeModel : IModelElement
{
    public int Width { get; init; }
    public int Height { get; }
}

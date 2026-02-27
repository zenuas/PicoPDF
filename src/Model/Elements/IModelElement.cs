using Binder.Data;

namespace PicoPDF.Model.Elements;

public interface IModelElement
{
    public IElement Element { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}

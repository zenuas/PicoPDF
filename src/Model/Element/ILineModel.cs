using System.Drawing;

namespace PicoPDF.Model.Element;

public interface ILineModel : IModelElement
{
    public int Width { get; init; }
    public int Height { get; }
    public Color? Color { get; init; }
    public int LineWidth { get; init; }
}

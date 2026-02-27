using System.Drawing;

namespace PicoPDF.Model.Elements;

public interface IRectangleModel : IPositionSizeModel
{
    public Color? Color { get; init; }
    public int LineWidth { get; init; }
}

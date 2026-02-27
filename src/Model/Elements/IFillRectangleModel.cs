using System.Drawing;

namespace PicoPDF.Model.Elements;

public interface IFillRectangleModel : IPositionSizeModel
{
    public Color LineColor { get; init; }
    public Color FillColor { get; init; }
    public int LineWidth { get; init; }
}

using System.Drawing;

namespace PicoPDF.Model.Elements;

public interface ILineModel : IPositionSizeModel
{
    public Color? Color { get; init; }
    public int LineWidth { get; init; }
}

using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public interface ILineModel : IModelElement
{
    public int Width { get; init; }
    public int Height { get; }
    public IColor? Color { get; init; }
    public int LineWidth { get; init; }
}

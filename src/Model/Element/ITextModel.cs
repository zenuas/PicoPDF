using PicoPDF.Binder.Element;
using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public interface ITextModel : IModelElement
{
    public string Text { get; }
    public int Size { get; init; }
    public string Font { get; init; }
    public TextAlignment Alignment { get; init; }
    public TextStyle Style { get; init; }
    public int Width { get; init; }
    public IColor? Color { get; init; }
}

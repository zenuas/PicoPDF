using PicoPDF.Loader.Elements;
using System.Drawing;

namespace PicoPDF.Model.Elements;

public interface ITextModel : IModelElement
{
    public string Text { get; }
    public int Size { get; init; }
    public string[] Font { get; init; }
    public TextAlignment Alignment { get; init; }
    public TextStyle Style { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public Color? Color { get; init; }
}

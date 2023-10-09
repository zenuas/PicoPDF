using System.Drawing;

namespace PicoPDF.Binder.Element;

public interface ITextElement : IElement
{
    public int Size { get; init; }
    public string Font { get; init; }
    public TextAlignment Alignment { get; init; }
    public TextStyle Style { get; init; }
    public int Width { get; init; }
    public Color? Color { get; init; }
}

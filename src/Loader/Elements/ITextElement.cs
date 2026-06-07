using Binder.Data;
using Pdf.Documents;
using PicoPDF.Loader.Sections;
using System.Drawing;

namespace PicoPDF.Loader.Elements;

public interface ITextElement : IElement
{
    public int Size { get; init; }
    public FontPath[] Font { get; init; }
    public TextAlignments Alignment { get; init; }
    public TextStyles Style { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public Color? Color { get; init; }
}

using Binder.Data;
using Pdf.Documents;
using PicoPDF.Loader.Sections;
using System.Drawing;

namespace PicoPDF.Model.Elements;

public record class TextModel : ITextModel
{
    public required IElement Element { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
    public required int Size { get; init; }
    public required FontPath[] Font { get; init; }
    public TextAlignments Alignment { get; init; } = TextAlignments.Start;
    public TextStyles Style { get; init; } = TextStyles.None;
    public int Width { get; init; }
    public int Height { get; init; }
    public Color? Color { get; init; }

    public override string ToString() => $"{Text}, X={X}, Y={Y}, Size={Size}";
}

using System.Drawing;

namespace PicoPDF.Binder.Element;

public class FillRectangleElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color LineColor { get; init; }
    public required Color FillColor { get; init; }
    public int LineWidth { get; init; } = 1;
}

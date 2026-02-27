using Binder.Data;
using System.Drawing;

namespace PicoPDF.Binder.Element;

public class CrossSectionRectangleElement : ICrossSectionElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public Color? Color { get; init; } = null;
    public int LineWidth { get; init; } = 1;
}

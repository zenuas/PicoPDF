using System.Drawing;

namespace PicoPDF.Binder.Element;

public class LineElement : ISectionElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public Color? Color { get; init; } = null;
}

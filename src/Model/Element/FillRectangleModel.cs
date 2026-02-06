using PicoPDF.Binder.Element;
using System.Drawing;

namespace PicoPDF.Model.Element;

public class FillRectangleModel : IPositionSizeModel
{
    public required IElement Element { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color LineColor { get; init; }
    public required Color FillColor { get; init; }
    public int LineWidth { get; init; }

    public override string ToString() => $"FillRectangle, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

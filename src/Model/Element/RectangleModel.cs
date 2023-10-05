using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public class RectangleModel : IModelElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public IColor? Color { get; init; }
    public int LineWidth { get; init; }

    public override string ToString() => $"Rectangle, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

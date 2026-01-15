using PicoPDF.Binder.Element;
using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public class LineModel : ILineModel
{
    public required IElement Element { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public IColor? Color { get; init; }
    public int LineWidth { get; init; }

    public override string ToString() => $"Line, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

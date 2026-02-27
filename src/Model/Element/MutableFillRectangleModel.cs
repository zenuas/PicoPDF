using Binder.Data;
using Binder.Model;
using System.Drawing;

namespace PicoPDF.Model.Element;

public class MutableFillRectangleModel : IFillRectangleModel, ICrossSectionModel
{
    public required IElement Element { get; init; }
    public ISectionModel? TargetSection { get; set; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; set; }
    public required Color LineColor { get; init; }
    public required Color FillColor { get; init; }
    public int LineWidth { get; init; }

    public override string ToString() => $"MutableFillRectangle, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

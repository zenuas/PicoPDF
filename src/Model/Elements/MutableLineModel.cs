using Binder.Data;
using Binder.Model;
using System.Drawing;

namespace PicoPDF.Model.Elements;

public class MutableLineModel : ILineModel, ICrossSectionModel<SectionModel>
{
    public required IElement Element { get; init; }
    public ISectionModel<SectionModel>? TargetSection { get; set; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; set; }
    public Color? Color { get; init; }
    public int LineWidth { get; init; }

    public override string ToString() => $"MutableLine, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

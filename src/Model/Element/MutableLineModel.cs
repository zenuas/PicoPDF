using PicoPDF.Binder.Element;
using PicoPDF.Pdf.Color;

namespace PicoPDF.Model.Element;

public class MutableLineModel : ILineModel, ICrossSectionModel
{
    public required IElement Element { get; init; }
    public SectionModel? TargetModel { get; set; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; set; }
    public IColor? Color { get; init; }
    public int LineWidth { get; init; }

    public void UpdatePosition(SectionModel current)
    {
        if (current.Section.Height < current.Top + Y + Height) Height = Height - current.Section.Height + TargetModel!.Top - current.Top;
    }

    public override string ToString() => $"MutableLine, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

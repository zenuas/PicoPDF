using PicoPDF.Binder.Element;
using System.Drawing;

namespace PicoPDF.Model.Element;

public class MutableLineModel : ILineModel, ICrossSectionModel
{
    public required IElement Element { get; init; }
    public SectionModel? TargetModel { get; set; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; set; }
    public Color? Color { get; init; }
    public int LineWidth { get; init; }

    public void UpdatePosition(SectionModel current)
    {
        if (current.Section.Height > current.Top + Y + Height) return;
        if (TargetModel!.Depth is { })
        {
            Height = Height - current.Section.Height + TargetModel.Top - current.Top;
        }
        else
        {
            Height = TargetModel.Top + TargetModel.Section.Height - current.Top - Y;
        }
    }

    public override string ToString() => $"MutableLine, X={X}, Y={Y}, Width={Width}, Height={Height}";
}

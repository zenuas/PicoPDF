namespace PicoPDF.Model.Element;

public interface ICrossSectionModel
{
    public SectionModel? TargetModel { get; set; }
    public int Y { get; init; }
    public int Height { get; set; }

    public void UpdatePosition(SectionModel current)
    {
        if (current.Section.Height > current.Top + Y + Height) return;
        if (TargetModel!.Depth is { } depth && current.Depth == depth)
        {
            Height = Height - current.Section.Height + TargetModel.Top - current.Top;
        }
        else
        {
            Height = TargetModel.Top + TargetModel.Section.Height - current.Top - Y;
        }
    }
}

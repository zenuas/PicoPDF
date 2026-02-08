namespace PicoPDF.Model.Element;

public interface ICrossSectionModel
{
    public SectionModel? TargetSection { get; set; }
    public int Y { get; init; }
    public int Height { get; set; }

    public void UpdatePosition(SectionModel current)
    {
        var overflow_height = Y + Height - current.Section.Height;
        if (overflow_height < 0) return;

        var world_top = current.Top + Y;
        int world_bottom = TargetSection!.Depth is { } depth && current.Depth == depth ? TargetSection.Top + overflow_height : TargetSection.Top + TargetSection.Section.Height;
        Height = world_bottom - world_top;
    }
}

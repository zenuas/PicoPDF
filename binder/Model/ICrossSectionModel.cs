namespace Binder.Model;

public interface ICrossSectionModel<TSection>
    where TSection : ISectionModel<TSection>
{
    public ISectionModel<TSection>? TargetSection { get; set; }
    public int Y { get; init; }
    public int Height { get; set; }

    public void UpdatePosition(ISectionModel<TSection> current)
    {
        var overflow_height = Y + Height - current.Height;
        if (overflow_height < 0) return;

        var world_top = current.Top + Y;
        int world_bottom = TargetSection!.Top + (TargetSection.Depth is { } depth && current.Depth == depth ? overflow_height : TargetSection.Height);
        Height = world_bottom - world_top;
    }
}

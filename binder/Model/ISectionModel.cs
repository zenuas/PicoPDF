using Binder.Data;

namespace Binder.Model;

public interface ISectionModel<TSection>
    where TSection : ISectionModel<TSection>
{
    public int Depth { get; init; }
    public int Top { get; set; }
    public int Height { get; init; }
    public bool IsFill { get; init; }
    public bool IsFooter { get; init; }
    public bool IsPageBreak { get; init; }
    public bool IsEmpty { get; init; }
    public bool IsVisible { get; init; }

    public void UpdatePosition();
    public static abstract TSection CreateSectionModel<T>(IPageSection<TSection> page, ISection section, T data, BindSummaryMapper<T, TSection> bind, int break_count, int? depth);
}

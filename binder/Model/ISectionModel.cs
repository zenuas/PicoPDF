using Binder.Data;

namespace Binder.Model;

public interface ISectionModel
{
    public int Depth { get; init; }
    public int Top { get; set; }
    public int Height { get; init; }
    public bool IsFooter { get; init; }

    public void UpdatePosition();

    public abstract static ISectionModel CreateSectionModel<T>(IPageSection page, ISection section, int left, T data, BindSummaryMapper<T> bind, int break_count, int? depth);
}

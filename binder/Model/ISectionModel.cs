using Binder.Data;

namespace Binder.Model;

public interface ISectionModel<TSection> : ISectionBaseModel
    where TSection : ISectionModel<TSection>
{
    public abstract static TSection CreateSectionModel<T>(IPageSection page, ISection section, T data, BindSummaryMapper<T, TSection> bind, int break_count, int? depth);
}

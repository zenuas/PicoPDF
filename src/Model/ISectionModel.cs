using PicoPDF.Binder;
using PicoPDF.Binder.Data;

namespace PicoPDF.Model;

public interface ISectionModel
{
    public int Depth { get; init; }
    public int Top { get; set; }
    public int Left { get; init; }
    public int Height { get; init; }
    public bool IsFooter { get; init; }

    public void UpdatePosition();

    public abstract static ISectionModel CreateSectionModel<T>(PageSection page, ISection section, int left, T data, BindSummaryMapper<T> bind, string[] breaks, string[] allkeys, int? depth);
}

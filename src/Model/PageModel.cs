using Binder.Model;
using System.Linq;

namespace PicoPDF.Model;

public class PageModel : IPageModel
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public SectionModel[] Models { get; init; } = [];

    public static IPageModel CreatePageModel<M>(int width, int height, M[] models)
        where M : ISectionModel
        => new PageModel()
        {
            Width = width,
            Height = height,
            Models = [.. models.OfType<SectionModel>()]
        };
}

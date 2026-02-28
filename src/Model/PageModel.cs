using Binder.Model;

namespace PicoPDF.Model;

public class PageModel : IPageModel<PageModel, SectionModel>
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public SectionModel[] Models { get; init; } = [];

    public static PageModel CreatePageModel(int width, int height, SectionModel[] models) => new()
    {
        Width = width,
        Height = height,
        Models = models,
    };
}

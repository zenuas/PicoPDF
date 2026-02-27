
using System;
using System.Linq;

namespace PicoPDF.Model;

public class PageModel : IPageModel
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public SectionModel[] Models { get; init; } = [];

    public static IPageModel CreatePageModel(int width, int height, ISectionModel[] models) => new PageModel()
    {
        Width = width,
        Height = height,
        Models = [.. models.Select(x => x as SectionModel ?? throw new InvalidCastException())]
    };
}

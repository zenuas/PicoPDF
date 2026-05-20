using Binder;
using Binder.Data;
using Binder.Model;
using Mina.Extension;
using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using PicoPDF.Model.Elements;
using System.Linq;

namespace PicoPDF.Model;

public record class SectionModel : ISectionModel<SectionModel>
{
    public required ISection Section { get; init; }
    public required int Depth { get; init; }
    public int Top { get; set; }
    public required int Left { get; init; }
    public required int Height { get; init; }
    public required bool IsFooter { get; init; }
    public required bool IsPageBreak { get; init; }
    public IModelElement[] Elements { get; init; } = [];
    public required int PageCount { get; init; }
    public required bool IsEmpty { get; init; }
    public required bool IsVisible { get; init; }

    public void UpdatePosition() => Elements
        .OfType<ICrossSectionModel<SectionModel>>()
        .Where(x => x.TargetSection is { })
        .Each(x => x.UpdatePosition(this));

    public static SectionModel CreateSectionModel<T>(IPageSection<SectionModel> page, ISection section, T data, BindSummaryMapper<T, SectionModel> bind, int break_count, int? depth)
    {
        var page_section = page.Cast<PageSection>();
        var footer = section as IFooterSection;
        return page_section.EventOption.BindSection(new()
        {
            Section = section,
            Depth = depth ?? 0,
            Left = page.Padding.Left,
            Height = section.Height,
            IsFooter = footer?.IsFooter ?? false,
            IsPageBreak = footer?.IsPageBreak ?? false,
            Elements = BindElements(section, data, bind, page_section, break_count, depth),
            PageCount = bind.GetPageCount(),
            IsEmpty = bind.IsEmpty,
            IsVisible = true,
        });
    }

    public static IModelElement[] BindElements<T>(ISection section, T data, BindSummaryMapper<T, SectionModel> bind, PageSection page, int break_count, int? depth) => [.. section.Elements.Select(x => page.EventOption.BindElement(section, x, data, BindElement(x, data, bind, page, break_count, depth)))];

    public static IModelElement BindElement<T>(IElement element, T data, BindSummaryMapper<T, SectionModel> bind, PageSection page, int break_count, int? depth)
    {
        switch (element)
        {
            case TextElement x: return CreateTextModel(x, x.Text, page.DefaultFont);

            case BindElement x: return CreateTextModel(x, BindSummaryMapper<T, SectionModel>.BindFormat(bind.Mapper[x.Bind](data), x.Format, x.Culture ?? page.DefaultCulture), page.DefaultFont);

            case SummaryElement x when x.SummaryMethod is SummaryMethod.All or SummaryMethod.Page or SummaryMethod.CrossSectionPage or SummaryMethod.Group:
                {
                    var model = CreateMutableTextModel(x, "", page.DefaultFont);
                    bind.AddSummaryGoBack(x, model, break_count);
                    return model;
                }

            case SummaryElement x: return CreateTextModel(x, BindSummaryMapper<T, SectionModel>.BindFormat(bind.GetSummary(x, data), x.Format, x.Culture ?? page.DefaultCulture, x.NaN), page.DefaultFont);

            case LineElement x:
                return new LineModel()
                {
                    Element = x,
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    Color = x.Color,
                    LineWidth = x.LineWidth,
                };

            case CrossSectionLineElement x:
                {
                    var model = new MutableLineModel()
                    {
                        Element = x,
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Color = x.Color,
                        LineWidth = x.LineWidth,
                    };
                    if (depth is { } dp) bind.AddCrossSectionGoBack(model, dp);
                    return model;
                }

            case RectangleElement x:
                return new RectangleModel()
                {
                    Element = x,
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    Color = x.Color,
                    LineWidth = x.LineWidth,
                };

            case CrossSectionRectangleElement x:
                {
                    var model = new MutableRectangleModel()
                    {
                        Element = x,
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Color = x.Color,
                        LineWidth = x.LineWidth,
                    };
                    if (depth is { } dp) bind.AddCrossSectionGoBack(model, dp);
                    return model;
                }

            case FillRectangleElement x:
                return new FillRectangleModel()
                {
                    Element = x,
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    LineColor = x.LineColor,
                    FillColor = x.FillColor,
                    LineWidth = x.LineWidth,
                };

            case CrossSectionFillRectangleElement x:
                {
                    var model = new MutableFillRectangleModel()
                    {
                        Element = x,
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        LineColor = x.LineColor,
                        FillColor = x.FillColor,
                        LineWidth = x.LineWidth,
                    };
                    if (depth is { } dp) bind.AddCrossSectionGoBack(model, dp);
                    return model;
                }

            case ImageElement x:
                return new ImageModel()
                {
                    Element = x,
                    X = x.X,
                    Y = x.Y,
                    Path = x.Bind == "" ? x.Path : bind.Mapper[x.Bind](data).ToString()!,
                    ZoomWidth = x.ZoomWidth,
                    ZoomHeight = x.ZoomHeight,
                };

            default:
                throw new();
        }
    }

    public static TextModel CreateTextModel(ITextElement element, string text, FontPath[] default_fonts) => new()
    {
        Element = element,
        X = element.X,
        Y = element.Y,
        Text = text,
        Font = [.. element.Font, .. default_fonts],
        Size = element.Size,
        Alignment = element.Alignment,
        Style = element.Style,
        Width = element.Width,
        Height = element.Height,
        Color = element.Color,
    };

    public static MutableTextModel CreateMutableTextModel(ITextElement element, string text, FontPath[] default_fonts) => new()
    {
        Element = element,
        X = element.X,
        Y = element.Y,
        Text = text,
        Font = [.. element.Font, .. default_fonts],
        Size = element.Size,
        Alignment = element.Alignment,
        Style = element.Style,
        Width = element.Width,
        Height = element.Height,
        Color = element.Color,
    };

    public override string ToString() => $"{Section.Name}, Top={Top}, Height={Height}, Elements={Elements.Length}";
}

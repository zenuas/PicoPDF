using Extensions;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Binder;

public static class SectionBinder
{
    public static PageModel[] Bind<T>(PageSection page, IEnumerable<T> datas) => BindPageModels(page, new BufferedEnumerator<T>() { BaseEnumerator = datas.GetEnumerator() })
        .Select(models =>
        {
            // header or detail top-down order
            int top = 0;
            models.Where(x => x.Section is not FooterSection).Each(x => x.Top = (top += x.Section.Height) - x.Section.Height);

            // footer bottom-up order
            int bottom = PdfUtility.GetPageSize(page.Size, page.Orientation).Height;
            models.Where(x => x.Section is FooterSection).Reverse().Each(x => x.Top = bottom -= x.Section.Height);

            return new PageModel() { Size = page.Size, Orientation = page.Orientation, Models = models };
        })
        .ToArray();

    public static List<List<SectionModel>> BindPageModels<T>(PageSection page, BufferedEnumerator<T> datas)
    {
        var sections = page.SubSection.Travers(x => x is Section s ? [s.SubSection] : []).ToArray();
        var detail = sections.OfType<DetailSection>().First();
        var headers = sections.OfType<Section>().Where(x => x.Header is { }).Select(x => (x.BreakKey, Section: (ISection)x.Header!)).PrependIf(page.Header).ToArray();
        var footers = sections.OfType<Section>().Where(x => x.Footer is { }).Select(x => (x.BreakKey, Section: (ISection)x.Footer!)).ToArray();
        var keys = headers.Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();

        var pages = new List<List<SectionModel>>();
        var models = new List<SectionModel>();
        var bind = new BindSummaryMapper<T>();
        bind.CreatePool(page);

        var everyfooter = page.Footer is FooterSection footer && footer.ViewMode == ViewModes.Every ? footer : null;
        var pageheight_minus_everypagefooter = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - (everyfooter?.Height ?? 0);
        var minimum_breakfooter_height = footers.SkipWhileOrEveryPage(_ => false).Select(x => x.Section.Height).Sum();

        while (!datas.IsLast)
        {
            bind.SetPageCount(pages.Count + 1);
            _ = datas.Next(0, out var lastdata);
            headers.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, lastdata, bind, page) }).Each(models.Add);
            var page_first = true;

            while (!datas.IsLast)
            {
                _ = datas.Next(0, out var current);
                var breakheader = page_first ? null : headers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](current)));
                var height = pageheight_minus_everypagefooter - (breakheader?.Select(x => x.Section.Height).Sum() ?? 0) - models.Select(x => x.Section.Height).Sum();
                var count = GetBreakOrTakeCount(datas, bind, keys, (height - minimum_breakfooter_height) / detail.Height);
                if (count == 0)
                {
                    if (everyfooter is { }) models.Add(new SectionModel() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page) });
                    break;
                }
                breakheader?.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, current, bind, page) }).Each(models.Add);

                var existnext = datas.Next(count, out var next);
                var breakfooter = (existnext ? footers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](current).Equals(bind.Mapper[x.BreakKey](next))) : footers)
                    .FooterSort()
                    .ToArray();

                if (height < (count * detail.Height) + breakfooter.Select(x => x.Section.Height).Sum())
                {
                    count--;
                    breakfooter = footers.SkipWhileOrEveryPage(_ => false).FooterSort().ToArray();
                }

                _ = datas.Next(count - 1, out lastdata);
                datas.GetRange(count).Select(x => new SectionModel() { Section = detail, Elements = BindElements(detail.Elements, x, bind.Return(y => y.DataBind(x)), page) }).Each(models.Add);
                breakfooter.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, lastdata, bind, page) }).Each(models.Add);
                if (breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak))
                {
                    if (everyfooter is { }) models.Add(new SectionModel() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page) });
                    if (existnext) bind.Clear(keys.TakeWhile(x => bind.Mapper[x](lastdata).Equals(bind.Mapper[x](next))).ToArray());
                    break;
                }
                if (existnext) bind.Clear(keys.TakeWhile(x => bind.Mapper[x](lastdata).Equals(bind.Mapper[x](next))).ToArray());
                page_first = false;
            }

            pages.Add(models.ToList());
            models.Clear();
            if (datas.IsLast && page.Footer is ISection lastfooter && lastfooter.ViewMode != ViewModes.Every) pages.Last().Add(new SectionModel() { Section = lastfooter, Elements = BindElements(lastfooter.Elements, lastdata, bind, page) });
        }
        return pages;
    }

    public static int GetBreakOrTakeCount<T>(BufferedEnumerator<T> datas, BindSummaryMapper<T> bind, string[] keys, int maxcount)
    {
        if (maxcount <= 0) return 0;

        _ = datas.Next(0, out var prev);
        var prevkey = keys.ToDictionary(x => x, x => bind.Mapper[x](prev));

        for (var i = 1; i < maxcount; i++)
        {
            if (!datas.Next(i, out var data) ||
                !keys.All(x => prevkey[x].Equals(bind.Mapper[x](data)))) return i;
        }
        return maxcount;
    }

    public static List<IModelElement> BindElements<T>(List<IElement> elements, T data, BindSummaryMapper<T> bind, PageSection page) => elements.Select(x => BindElement(x, data, bind, page)).ToList();

    public static IModelElement BindElement<T>(IElement element, T data, BindSummaryMapper<T> bind, PageSection page)
    {
        switch (element)
        {
            case TextElement x:
                return new TextModel()
                {
                    X = x.X,
                    Y = x.Y,
                    Text = x.Text,
                    Font = x.Font != "" ? x.Font : page.DefaultFont,
                    Size = x.Size,
                };

            case BindElement x:
                {
                    var o = bind.Mapper[x.Bind](data);
                    return new TextModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Text = (x.Format == "" ? o?.ToString() : o?.Cast<IFormattable>()?.ToString(x.Format, null)) ?? "",
                        Font = x.Font != "" ? x.Font : page.DefaultFont,
                        Size = x.Size,
                        Alignment = x.Alignment,
                        Width = x.Width,
                    };
                }

            case SummaryElement x:
                {
                    var o = bind.GetSummary(x, data);
                    return new TextModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Text = (x.Format == "" ? o?.ToString() : o?.Cast<IFormattable>()?.ToString(x.Format, null)) ?? "",
                        Font = x.Font != "" ? x.Font : page.DefaultFont,
                        Size = x.Size,
                        Alignment = x.Alignment,
                        Width = x.Width,
                    };
                }

            case LineElement x:
                {
                    return new LineModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Color = x.Color is { } color ? new DeviceRGB() { R = (double)color.R / 255, G = (double)color.G / 255, B = (double)color.B / 255 } : null,
                    };
                }

            case RectangleElement x:
                {
                    return new RectangleModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Color = x.Color is { } color ? new DeviceRGB() { R = (double)color.R / 255, G = (double)color.G / 255, B = (double)color.B / 255 } : null,
                    };
                }

            case FillRectangleElement x:
                {
                    return new FillRectangleModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        LineColor = new DeviceRGB() { R = (double)x.LineColor.R / 255, G = (double)x.LineColor.G / 255, B = (double)x.LineColor.B / 255 },
                        FillColor = new DeviceRGB() { R = (double)x.FillColor.R / 255, G = (double)x.FillColor.G / 255, B = (double)x.FillColor.B / 255 },
                    };
                }

            case ImageElement x:
                {
                    var path = x.Bind == "" ? x.Path : bind.Mapper[x.Bind](data).ToString()!;

                    return new ImageModel()
                    {
                        X = x.X,
                        Y = x.Y,
                        Path = path,
                        ZoomWidth = x.ZoomWidth,
                        ZoomHeight = x.ZoomHeight,
                    };
                }
        }
        throw new();
    }

    public static IEnumerable<(string BreakKey, ISection Section)> SkipWhileOrPageFirst(this IEnumerable<(string BreakKey, ISection Section)> self, Func<(string BreakKey, ISection Section), bool> f)
    {
        var found = false;
        foreach (var x in self)
        {
            if (!found && f(x)) found = true;
            if (found || x.Section.ViewMode == ViewModes.Every || x.Section.ViewMode == ViewModes.PageFirst)
            {
                yield return x;
            }
        }
    }

    public static IEnumerable<(string BreakKey, ISection Section)> SkipWhileOrEveryPage(this IEnumerable<(string BreakKey, ISection Section)> self, Func<(string BreakKey, ISection Section), bool> f)
    {
        var found = false;
        foreach (var x in self)
        {
            if (!found && f(x)) found = true;
            if (found || x.Section.ViewMode == ViewModes.Every)
            {
                yield return x;
            }
        }
    }

    public static IEnumerable<(string BreakKey, ISection Section)> FooterSort(this IEnumerable<(string BreakKey, ISection Section)> self)
    {
        var footer = self.ToArray();
        return footer
            .Where(x => x.Section is TotalSection)
            .Reverse()
            .Concat(footer.Where(x => x.Section is FooterSection));
    }

    public static IEnumerable<(string BreakKey, ISection Section)> PrependIf(this IEnumerable<(string BreakKey, ISection Section)> self, IHeaderSection? header)
    {
        if (header is ISection section) return self.Prepend(("", section));
        return self;
    }
}

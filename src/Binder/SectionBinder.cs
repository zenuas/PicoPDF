using Extensions;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Binder.Element;
using PicoPDF.Binder.Data;
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
        var pageheight_minus_everypagefooter = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - everyfooter?.Height ?? 0;
        var minimum_breakfooter_height = footers.SkipWhileOrEveryPage(_ => false).Sum(x => x.Section.Height);
        var rest_section = new List<SectionModel>();

        while (!datas.IsLast)
        {
            _ = datas.Next(0, out var current);
            headers.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, current, bind, page) }).Each(models.Add);
            rest_section.Each(models.Add);
            rest_section.Clear();

            _ = datas.Next(0, out var lastdata);
            while (!datas.IsLast)
            {
                var height = pageheight_minus_everypagefooter - models.Sum(x => x.Section.Height);
                var count = GetBreakOrTakeCount(datas, bind, keys, (height - minimum_breakfooter_height) / detail.Height);
                if (count == 0)
                {
                    if (everyfooter is { }) models.Add(new SectionModel() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page) });
                    break;
                }

                _ = datas.Next(0, out current);
                var existnext = datas.Next(count, out var next);
                var breakfooter = (existnext ? footers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](current).Equals(bind.Mapper[x.BreakKey](next))) : footers)
                    .FooterSort()
                    .ToArray();

                if (height < (count * detail.Height) + breakfooter.Sum(x => x.Section.Height))
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
            }

            pages.Add(models.ToList());
            models.Clear();
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

    public static List<IModelElement> BindElements<T>(List<ISectionElement> elements, T data, BindSummaryMapper<T> bind, PageSection page) => elements.Select(x => BindElement(x, data, bind, page)).ToList();

    public static IModelElement BindElement<T>(ISectionElement element, T data, BindSummaryMapper<T> bind, PageSection page)
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

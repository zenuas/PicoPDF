using Extensions;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Section;

public static class SectionBinder
{
    public static PageModel[] Bind<T>(PageSection page, IEnumerable<T> datas)
    {
        var pageheight = PdfUtility.GetPageSize(page.Size, page.Orientation).Height;
        var sections = page.SubSection
            .Travers(x => x is Section s ? [s.SubSection] : [])
            .ToArray();
        var detail = sections.OfType<DetailSection>().First();
        var headers = sections.OfType<Section>().Where(x => x.Header is { }).Select(x => (x.BreakKey, Section: (ISection)x.Header!)).ToArray();
        var footers = sections.OfType<Section>().Where(x => x.Footer is { }).Select(x => (x.BreakKey, Section: (ISection)x.Footer!)).ToArray();
        var pages = new List<PageModel>();
        var models = new List<SectionModel>();

        var bind = new BindSummaryMapper<T>();
        bind.CreatePool(page);

        var keys = headers.Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();
        Dictionary<string, object>? prevkey = null;
        T? prevdata = default;
        var pageheight_minus_everypagefooter = pageheight - (page.Footer is FooterSection everyfooter && everyfooter.ViewMode == ViewModes.Every ? everyfooter.Height : 0);
        var pos = new TopBottom() { Bottom = pageheight_minus_everypagefooter };

        foreach (var data in datas)
        {
            var keyset = keys.ToDictionary(x => x, x => bind.Mapper[x](data));
            if (prevkey is null || !keys.All(x => prevkey[x].Equals(keyset[x])))
            {
                if (prevkey is { })
                {
                    var breakfooter = footers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && !prevkey[x.BreakKey].Equals(keyset[x.BreakKey]))
                        .Reverse()
                        .ToArray();
                    var pagebreak = breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak);

                    if (pagebreak && page.Footer is FooterSection pagefooter && pagefooter.ViewMode == ViewModes.Every)
                    {
                        pos.Bottom = pageheight;
                        models.Add(SectionToModel(pagefooter, pos, prevdata!, bind));
                    }
                    models.AddRange(breakfooter.Select(x => SectionToModel(x.Section, pos, prevdata!, bind)));

                    if (pagebreak)
                    {
                        if (page.Footer is TotalSection pagetotal && pagetotal.ViewMode == ViewModes.Every) models.Add(SectionToModel(pagetotal, pos, prevdata!, bind));
                        pages.Add(ModelsToPage(page, models));
                        models.Clear();
                    }

                    bind.Clear(keys.TakeWhile(x => prevkey[x].Equals(keyset[x])).ToArray());
                    bind.DataBind(data);

                    if (pagebreak)
                    {
                        pos.Top = 0;
                        pos.Bottom = pageheight_minus_everypagefooter;
                        if (page.Header is ISection pageheader && pageheader.ViewMode == ViewModes.Every) models.Add(SectionToModel(pageheader, pos, prevdata!, bind));
                    }

                    models.AddRange(headers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && !prevkey[x.BreakKey].Equals(keyset[x.BreakKey]))
                        .Select(x => SectionToModel(x.Section, pos, data, bind)));
                }
                else
                {
                    bind.DataBind(data);
                    if (page.Header is ISection pageheader) models.Add(SectionToModel(pageheader, pos, prevdata!, bind));
                    models.AddRange(headers.Select(x => SectionToModel(x.Section, pos, data, bind)));
                }
                prevkey = keyset;
            }
            else
            {
                bind.DataBind(data);
            }
            models.Add(SectionToModel(detail, pos, data, bind));
            prevdata = data;
        }

        if (page.Footer is FooterSection lastfooter)
        {
            pos.Bottom = pageheight;
            models.Add(SectionToModel(lastfooter, pos, prevdata!, bind));
        }
        models.AddRange(footers.Reverse().Select(x => SectionToModel(x.Section, pos, prevdata!, bind)));
        if (page.Footer is TotalSection lasttotal) models.Add(SectionToModel(lasttotal, pos, prevdata!, bind));
        pages.Add(ModelsToPage(page, models));

        return pages.ToArray();
    }

    public static SectionModel SectionToModel<T>(ISection section, TopBottom pos, T data, BindSummaryMapper<T> bind) => new()
    {
        Section = section,
        Top = section is FooterSection ? (pos.Bottom -= section.Height) : (pos.Top += section.Height) - section.Height,
        Elements = BindElements(section.Elements, data, bind).ToList(),
    };

    public static PageModel ModelsToPage(PageSection page, List<SectionModel> models) => new()
    {
        Size = page.Size,
        Orientation = page.Orientation,
        DefaultFont = page.DefaultFont,
        Models = models.ToList(),
    };

    public static IEnumerable<IModelElement> BindElements<T>(List<ISectionElement> elements, T data, BindSummaryMapper<T> bind) => elements.Select(x => BindElement(x, data, bind));

    public static IModelElement BindElement<T>(ISectionElement element, T data, BindSummaryMapper<T> bind)
    {
        switch (element)
        {
            case TextElement x:
                return new TextModel()
                {
                    X = x.X,
                    Y = x.Y,
                    Text = x.Text,
                    Font = x.Font,
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
                        Font = x.Font,
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
                        Font = x.Font,
                        Size = x.Size,
                    };
                }
        }
        throw new();
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
}

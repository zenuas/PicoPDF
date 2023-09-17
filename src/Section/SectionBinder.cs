using Extensions;
using PicoPDF.Document;
using PicoPDF.Mapper;
using PicoPDF.Model;
using PicoPDF.Model.Element;
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

        var mapper = ObjectMapper.CreateGetMapper<T>();
        var summaries = TraversSummaryElement([], page);
        var summarypool = new Dictionary<string, dynamic>();
        var summaryaction = new List<Action<T>>();
        summaries.Each(sr =>
        {
            var key = sr.BreakKeys.Join(".");
            var bind = sr.SummaryElement.Bind;
            var sumkey = $"${key}:{bind}";
            var countkey = $"#{key}:";
            if (summarypool.TryAdd(sumkey, 0))
            {
                mapper.Add(sumkey, _ => summarypool[sumkey]);
                summaryaction.Add(x => summarypool[sumkey] += (dynamic)mapper[bind](x));
            }
            if (summarypool.TryAdd(countkey, 0))
            {
                mapper.Add(countkey, _ => summarypool[countkey]);
                summaryaction.Add(x => summarypool[countkey]++);
            }
            sr.SummaryElement.Bind = sumkey;
        });

        var keys = headers.Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();
        Dictionary<string, object>? prevkey = null;
        T? prevdata = default;

        foreach (var data in datas)
        {
            var keyset = keys.ToDictionary(x => x, x => mapper[x](data));
            if (prevkey is null || !keys.All(x => prevkey[x].Equals(keyset[x])))
            {
                if (prevkey is { })
                {
                    var breakfooter = footers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && !prevkey[x.BreakKey].Equals(keyset[x.BreakKey]))
                        .Reverse()
                        .ToArray();
                    var pagebreak = breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak);

                    var top = models.Last().Top + models.Last().Height;
                    var height = pageheight;
                    models.AddRange(breakfooter
                        .Select(x => new SectionModel() { Name = x.Section.Name, Top = (height -= x.Section.Height), Height = x.Section.Height, Elements = BindElements(x.Section.Elements, prevdata!, mapper).ToList() }));

                    if (pagebreak)
                    {
                        if (page.Footer is ISection pagefooter) models.Add(new SectionModel() { Name = pagefooter.Name, Top = (height -= pagefooter.Height), Height = pagefooter.Height, Elements = BindElements(pagefooter.Elements, prevdata!, mapper).ToList() });
                        pages.Add(new() { Size = page.Size, Orientation = page.Orientation, DefaultFont = page.DefaultFont, Models = models.ToList() });
                        models.Clear();
                    }

                    var nobreak = keys.TakeWhile(x => prevkey[x].Equals(keyset[x])).Join(".");
                    var sumkey_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
                    var countkey_prefix = $"#{(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
                    summarypool.Keys.Where(x => x.StartsWith(sumkey_prefix) || x.StartsWith(countkey_prefix)).Each(x => summarypool[x] = 0);
                    summaryaction.Each(x => x(data));

                    if (pagebreak)
                    {
                        top = 0;
                        if (page.Header is ISection pageheader) models.Add(new SectionModel() { Name = pageheader.Name, Top = (top += pageheader.Height) - pageheader.Height, Height = pageheader.Height, Elements = BindElements(pageheader.Elements, prevdata!, mapper).ToList() });
                    }

                    models.AddRange(headers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && !prevkey[x.BreakKey].Equals(keyset[x.BreakKey]))
                        .Select(x => new SectionModel() { Name = x.Section.Name, Top = (top += x.Section.Height) - x.Section.Height, Height = x.Section.Height, Elements = BindElements(x.Section.Elements, data, mapper).ToList() }));
                }
                else
                {
                    summaryaction.Each(x => x(data));
                    var top = 0;
                    if (page.Header is ISection pageheader) models.Add(new SectionModel() { Name = pageheader.Name, Top = (top += pageheader.Height) - pageheader.Height, Height = pageheader.Height, Elements = BindElements(pageheader.Elements, prevdata!, mapper).ToList() });
                    models.AddRange(headers
                        .Select(x => new SectionModel() { Name = x.Section.Name, Top = (top += x.Section.Height) - x.Section.Height, Height = x.Section.Height, Elements = BindElements(x.Section.Elements, data, mapper).ToList() }));
                }
                prevkey = keyset;
            }
            else
            {
                summaryaction.Each(x => x(data));
            }
            var last = models.Last();
            models.Add(new SectionModel() { Name = detail.Name, Top = (last?.Top ?? 0) + (last?.Height ?? 0), Height = detail.Height, Elements = BindElements(detail.Elements, data, mapper).ToList() });

            prevdata = data;
        }

        var lastpage = pageheight;
        models.AddRange(footers
            .Reverse()
            .Select(x => new SectionModel() { Name = x.Section.Name, Top = (lastpage -= x.Section.Height), Height = x.Section.Height, Elements = BindElements(x.Section.Elements, prevdata!, mapper).ToList() }));
        if (page.Footer is ISection lastfooter) models.Add(new SectionModel() { Name = lastfooter.Name, Top = (lastpage -= lastfooter.Height), Height = lastfooter.Height, Elements = BindElements(lastfooter.Elements, prevdata!, mapper).ToList() });
        pages.Add(new() { Size = page.Size, Orientation = page.Orientation, DefaultFont = page.DefaultFont, Models = models.ToList() });

        return pages.ToArray();
    }

    public static IEnumerable<IModelElement> BindElements<T>(List<ISectionElement> elements, T data, Dictionary<string, Func<T, object>> mapper) => elements.Select(x => BindElement(x, data, mapper));

    public static IModelElement BindElement<T>(ISectionElement element, T data, Dictionary<string, Func<T, object>> mapper)
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
                    var o = mapper[x.Bind](data);
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
                    var o = mapper[x.Bind](data);
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
        throw new Exception();
    }

    public static IEnumerable<(string BreakKey, ISection Section)> SkipWhileOrEveryPage(this IEnumerable<(string BreakKey, ISection Section)> self, Func<(string BreakKey, ISection Section), bool> f)
    {
        var found = false;
        foreach (var x in self)
        {
            if (!found && f(x)) found = true;
            if (found || (x.Section.ViewMode & ViewModes.MODES) == ViewModes.Every)
            {
                yield return x;
            }
        }
    }

    public static List<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, IParentSection section)
    {
        if (section is Section x && x.BreakKey != "") keys = keys.Append(x.BreakKey).ToArray();

        var results = new List<(string[] BreakKeys, SummaryElement SummaryElement)>();
        if (section.Header is ISection header) results.AddRange(TraversSummaryElement(keys, header.Elements));
        if (section.SubSection is ISection detail) results.AddRange(TraversSummaryElement(keys, detail.Elements));
        if (section.SubSection is IParentSection subsection) results.AddRange(TraversSummaryElement(keys, subsection));
        if (section.Footer is ISection footer) results.AddRange(TraversSummaryElement(keys, footer.Elements));
        return results;
    }

    public static IEnumerable<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, List<ISectionElement> elements) => elements.OfType<SummaryElement>().Select(x => (keys, x));
}

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
        var headers = sections.OfType<Section>().Where(x => x.Header is { }).Select(x => (x.BreakKey, Section: (ISection)x.Header!)).Then(_ => page.Header is { }, x => x.Prepend((BreakKey: "", Section: (ISection)page.Header!))).ToArray();
        var footers = sections.OfType<Section>().Where(x => x.Footer is { }).Select(x => (x.BreakKey, Section: (ISection)x.Footer!)).Then(_ => page.Footer is { }, x => x.Prepend((BreakKey: "", Section: (ISection)page.Footer!))).ToArray();
        var keys = headers.Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();
        var mapper = ObjectMapper.CreateGetMapper<T>();
        var pages = new List<PageModel>();
        var models = new List<SectionModel>();

        Dictionary<string, object>? prevkey = null;
        T? prevdata = default;

        foreach (var data in datas)
        {
            var keyset = keys.ToDictionary(x => x, x => mapper[x](data));
            if (prevkey is null || !keys.All(x => prevkey[x].Equals(keyset[x])))
            {
                if (prevkey is { })
                {
                    var height = pageheight;
                    models.AddRange(footers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && prevkey[x.BreakKey] != keyset[x.BreakKey])
                        .Reverse()
                        .Select(x => new SectionModel() { Top = (height -= x.Section.Height), Height = x.Section.Height, Elements = BindElements(x.Section.Elements, prevdata!, mapper).ToList() }));
                    pages.Add(new() { Size = page.Size, Orientation = page.Orientation, Models = models.ToList() });
                    models.Clear();

                    var top = 0;
                    models.AddRange(headers
                        .SkipWhileOrEveryPage(x => x.BreakKey != "" && prevkey[x.BreakKey] != keyset[x.BreakKey])
                        .Select(x => new SectionModel() { Top = (top += x.Section.Height) - x.Section.Height, Height = x.Section.Height, Elements = BindElements(x.Section.Elements, data, mapper).ToList() }));
                }
                else
                {
                    var top = 0;
                    models.AddRange(headers
                        .Select(x => new SectionModel() { Top = (top += x.Section.Height) - x.Section.Height, Height = x.Section.Height, Elements = BindElements(x.Section.Elements, data, mapper).ToList() }));
                }
                prevkey = keyset;
            }
            var last = models.Last();
            models.Add(new SectionModel() { Top = (last?.Top ?? 0) + (last?.Height ?? 0), Height = detail.Height, Elements = BindElements(detail.Elements, data, mapper).ToList() });

            prevdata = data;
        }

        var lastpage = pageheight;
        models.AddRange(footers
            .Reverse()
            .Select(x => new SectionModel() { Top = (lastpage -= x.Section.Height), Height = x.Section.Height, Elements = BindElements(x.Section.Elements, prevdata!, mapper).ToList() }));
        pages.Add(new() { Size = page.Size, Orientation = page.Orientation, Models = models.ToList() });

        return pages.ToArray();
    }

    public static IEnumerable<IModelElement> BindElements<T>(List<ISectionElement> elements, T data, Dictionary<string, Func<T, object>> mapper) => elements.Select(x => BindElement(x, data, mapper));

    public static IModelElement BindElement<T>(ISectionElement element, T data, Dictionary<string, Func<T, object>> mapper)
    {
        switch (element)
        {
            case TextElement x:
                return new TextModel() { X = x.X, Y = x.Y, Text = x.Text };

            case BindElement x:
                var o = mapper[x.Bind](data);
                return new TextModel() { X = x.X, Y = x.Y, Text = (x.Format == "" ? o?.ToString() : o?.Cast<IFormattable>()?.ToString(x.Format, null)) ?? "" };
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
}

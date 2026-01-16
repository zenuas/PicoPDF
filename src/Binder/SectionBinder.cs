using Mina.Extension;
using Mina.Mapper;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PicoPDF.Binder;

public static class SectionBinder
{
    public static PageModel[] Bind<T>(PageSection page, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null) => [.. BindPageModels(page, new BufferedEnumerator<T>() { BaseEnumerator = datas.GetEnumerator() }, mapper ?? InstanceMapper.CreateGetMapper<T>())
        .Select(models =>
        {
            // header or detail top-down order
            int top = page.Padding.Top;
            models.Where(x => x.Section is not FooterSection).Each(x => x.Top = (top += x.Section.Height) - x.Section.Height);

            // footer bottom-up order
            int bottom = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - page.Padding.Bottom;
            models.Where(x => x.Section is FooterSection).Reverse().Each(x => x.Top = bottom -= x.Section.Height);
            
            // cross section update position
            foreach(var model in models)
            {
                model.Elements
                    .OfType<ICrossSectionModel>()
                    .Where(x => x.TargetModel is { })
                    .Each(x => x.UpdatePosition(model));
            }

            return new PageModel() { Size = page.Size, Orientation = page.Orientation, Models = models };
        })];

    public static PageModel[] Bind(PageSection page, DataTable table) => Bind(page,
        table.Rows.GetIterator().OfType<DataRow>(),
        table.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRow, object>>(x => x.ColumnName, x => (row) => row?[x]!));

    public static PageModel[] Bind(PageSection page, DataView view) => Bind(page,
        view.GetIterator().OfType<DataRowView>(),
        view.Table!.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRowView, object>>(x => x.ColumnName, x => (row) => row?[x.ColumnName]!));

    public static List<List<SectionModel>> BindPageModels<T>(PageSection page, BufferedEnumerator<T> datas, Dictionary<string, Func<T, object>> mapper)
    {
        var sections = (Section: page.SubSection, Level: 1).Travers(x => x.Section is Section s ? [(s.SubSection, x.Level + 1)] : []).ToArray();
        var detail = sections.Select(x => x.Section).OfType<DetailSection>().First();
        var hierarchy = sections.Where(x => x.Section is Section).Select(x => (x.Section.Cast<Section>(), x.Level)).AppendHierarchy().ToArray();
        var headers = hierarchy.Where(x => x.Section.Header is { }).Select(x => new SectionInfo(x.Section.BreakKey, x.BreakKeyHierarchy, Section: x.Section.Header!, x.Level)).PrependIf(page.Header, 0).ToArray();
        var footers = hierarchy.Where(x => x.Section.Footer is { }).Select(x => new SectionInfo(x.Section.BreakKey, x.BreakKeyHierarchy, Section: x.Section.Footer!, x.Level)).ToArray();
        var keys = sections.Select(x => x.Section).OfType<Section>().Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();

        var bind = new BindSummaryMapper<T>() { Mapper = mapper };
        bind.CreatePool(page, keys);
        bind.CreateSummaryGoBack(keys.Length);
        bind.CreateCrossSectionGoBack(headers.LastOrDefault()?.Level ?? 0);

        T lastdata = default!;

        var models = new List<SectionModel>();
        if (datas.IsLast)
        {
            bind.SetPageCount(1);
            headers.Select(x => new SectionModel() { Section = x.Section, Level = x.Level, Elements = BindElements(x.Section.Elements, lastdata, bind, page, x.BreakKeyHierarchy, keys, x.Level) }).Each(models.Add);
            footers.Select(x => new SectionModel() { Section = x.Section, Level = x.Level, Elements = BindElements(x.Section.Elements, lastdata, bind, page, x.BreakKeyHierarchy, keys, null) }).Each(models.Add);
            if (page.Footer is ISection lastfooter) models.Add(new() { Section = lastfooter, Level = 0, Elements = BindElements(lastfooter.Elements, lastdata, bind, page, [], keys, null) });
            bind.KeyBreak(lastdata, keys.Length, keys, page);
            bind.PageBreak(lastdata, page);
            bind.LastBreak(lastdata, page);
            return [models];
        }

        var pages = new List<List<SectionModel>>();
        var everyfooter = page.Footer is FooterSection footer && footer.ViewMode == ViewModes.Every ? footer : null;
        var pageheight_minus_everypagefooter = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - page.Padding.Top - page.Padding.Bottom - (everyfooter?.Height ?? 0);
        var minimum_breakfooter_height = footers.SkipWhileOrEveryPage(_ => false).Select(x => x.Section.Height).Sum();
        while (!datas.IsLast)
        {
            bind.SetPageCount(pages.Count + 1);
            _ = datas.Next(0, out var firstdata);
            if (pages.Count == 0) lastdata = firstdata;
            headers
                .SkipWhileOrPageFirst(x => pages.Count == 0 || (x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](firstdata))))
                .Select(x => new SectionModel() { Section = x.Section, Level = x.Level, Elements = BindElements(x.Section.Elements, firstdata, bind, page, x.BreakKeyHierarchy, keys, x.Level) })
                .Each(models.Add);
            var page_first = true;
            var breakcount = 0;

            while (true)
            {
                _ = datas.Next(0, out var current);
                var breakheader = page_first ? null : headers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](current)));
                var height = pageheight_minus_everypagefooter - (breakheader?.Select(x => x.Section.Height).Sum() ?? 0) - models.Select(x => x.Section.Height).Sum();
                var count = GetBreakOrTakeCount(datas, bind, keys, (height - minimum_breakfooter_height) / detail.Height);
                if (count == 0)
                {
                    if (everyfooter is { }) models.Add(new() { Section = everyfooter, Level = 0, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys, null) });
                    break;
                }

                var existnext = datas.Next(count, out var next);
                breakcount = keys.Length - (existnext ? keys.TakeWhile(x => bind.Mapper[x](current).Equals(bind.Mapper[x](next))).Count() : 0);
                var breakfooter = (existnext ? footers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](current).Equals(bind.Mapper[x.BreakKey](next))) : footers)
                    .FooterSort()
                    .ToArray();

                if (height < (count * detail.Height) + breakfooter.Select(x => x.Section.Height).Sum())
                {
                    if (--count <= 0)
                    {
                        if (everyfooter is { }) models.Add(new() { Section = everyfooter, Level = 0, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys, null) });
                        break;
                    }
                    breakcount = 0;
                    breakfooter = [.. footers.SkipWhileOrEveryPage(_ => false).FooterSort()];
                }
                if (!page_first) bind.SectionBreak(lastdata, page);
                breakheader?.Select(x => new SectionModel() { Section = x.Section, Level = x.Level, Elements = BindElements(x.Section.Elements, current, bind, page, x.BreakKeyHierarchy, keys, x.Level) }).Each(models.Add);

                _ = datas.Next(count - 1, out lastdata);
                datas.GetRange(count).Select(x => new SectionModel() { Section = detail, Level = null, Elements = BindElements(detail.Elements, x, bind.Return(y => y.DataBind(x)), page, keys, keys, null) }).Each(models.Add);
                breakfooter.Select(x => new SectionModel() { Section = x.Section, Level = x.Level, Elements = BindElements(x.Section.Elements, lastdata, bind, page, x.BreakKeyHierarchy, keys, null) }).Each(models.Add);
                if (breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak))
                {
                    if (everyfooter is { }) models.Add(new() { Section = everyfooter, Level = 0, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys, null) });
                    if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys, page);
                    break;
                }
                if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys, page);
                page_first = false;

                if (datas.IsLast)
                {
                    if (page.Footer is ISection lastfooter) models.Add(new() { Section = lastfooter, Level = 0, Elements = BindElements(lastfooter.Elements, lastdata, bind, page, [], keys, null) });
                    break;
                }
            }

            pages.Add([.. models]);
            models.Clear();
            bind.PageBreak(lastdata, page);
            if (datas.IsLast) bind.LastBreak(lastdata, page);
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

    public static List<IModelElement> BindElements<T>(List<IElement> elements, T data, BindSummaryMapper<T> bind, PageSection page, string[] keys, string[] allkeys, int? level) => [.. elements.Select(x => BindElement(x, data, bind, page, keys, allkeys, level))];

    public static IModelElement BindElement<T>(IElement element, T data, BindSummaryMapper<T> bind, PageSection page, string[] keys, string[] allkeys, int? level)
    {
        var posx = element.X + page.Padding.Left;
        switch (element)
        {
            case TextElement x: return CreateTextModel(x, x.Text, page);

            case BindElement x: return CreateTextModel(x, BindSummaryMapper<T>.BindFormat(bind.Mapper[x.Bind](data), x.Format, x.Culture ?? page.DefaultCulture), page);

            case SummaryElement x when x.SummaryMethod is SummaryMethod.All or SummaryMethod.Page or SummaryMethod.CrossSectionPage or SummaryMethod.Group:
                {
                    var model = CreateMutableTextModel(x, "", page);
                    var keycount = x.BreakKey == "" ? keys.Length - 1 : allkeys.FindLastIndex(y => y == x.BreakKey);
                    bind.AddSummaryGoBack(x, model, keycount);
                    return model;
                }

            case SummaryElement x: return CreateTextModel(x, BindSummaryMapper<T>.BindFormat(bind.GetSummary(x, data), x.Format, x.Culture ?? page.DefaultCulture, x.NaN), page);

            case LineElement x:
                return new LineModel()
                {
                    Element = x,
                    X = posx,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    Color = x.Color?.ToDeviceRGB(),
                    LineWidth = x.LineWidth,
                };

            case CrossSectionLineElement x:
                {
                    var model = new MutableLineModel()
                    {
                        Element = x,
                        X = posx,
                        Y = x.Y,
                        Width = x.Width,
                        Height = x.Height,
                        Color = x.Color?.ToDeviceRGB(),
                        LineWidth = x.LineWidth,
                    };
                    if (level is { } lv) bind.AddCrossSectionGoBack(model, lv);
                    return model;
                }

            case RectangleElement x:
                return new RectangleModel()
                {
                    Element = x,
                    X = posx,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    Color = x.Color?.ToDeviceRGB(),
                    LineWidth = x.LineWidth,
                };

            case FillRectangleElement x:
                return new FillRectangleModel()
                {
                    Element = x,
                    X = posx,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    LineColor = x.LineColor.ToDeviceRGB(),
                    FillColor = x.FillColor.ToDeviceRGB(),
                    LineWidth = x.LineWidth,
                };

            case ImageElement x:
                return new ImageModel()
                {
                    Element = x,
                    X = posx,
                    Y = x.Y,
                    Path = x.Bind == "" ? x.Path : bind.Mapper[x.Bind](data).ToString()!,
                    ZoomWidth = x.ZoomWidth,
                    ZoomHeight = x.ZoomHeight,
                };
        }
        throw new();
    }

    public static TextModel CreateTextModel(ITextElement element, string text, PageSection page) => new()
    {
        Element = element,
        X = element.X + page.Padding.Left,
        Y = element.Y,
        Text = text,
        Font = element.Font != "" ? element.Font : page.DefaultFont,
        Size = element.Size,
        Alignment = element.Alignment,
        Style = element.Style,
        Width = element.Width,
        Color = element.Color?.ToDeviceRGB(),
    };

    public static MutableTextModel CreateMutableTextModel(ITextElement element, string text, PageSection page) => new()
    {
        Element = element,
        X = element.X + page.Padding.Left,
        Y = element.Y,
        Text = text,
        Font = element.Font != "" ? element.Font : page.DefaultFont,
        Size = element.Size,
        Alignment = element.Alignment,
        Style = element.Style,
        Width = element.Width,
        Color = element.Color?.ToDeviceRGB(),
    };

    public static IEnumerable<(string[] BreakKeys, SummaryElement Summary)> GetBreakKeyWithSummary(string[] keys, ISubSection subsection)
    {
        if (subsection is DetailSection detail)
        {
            foreach (var e in detail.Elements.OfType<SummaryElement>()) yield return (keys, e);
        }
        else if (subsection is Section section)
        {
            var newkeys = section.BreakKey == "" ? keys : [.. keys, section.BreakKey];

            foreach (var e in section.Header?.Elements.OfType<SummaryElement>() ?? []) yield return (newkeys, e);
            foreach (var e in section.Footer?.Elements.OfType<SummaryElement>() ?? []) yield return (newkeys, e);

            if (section.SubSection is { })
            {
                foreach (var x in GetBreakKeyWithSummary(newkeys, section.SubSection)) yield return x;
            }
        }
    }

    public static IEnumerable<SectionInfo> SkipWhileOrPageFirst(this IEnumerable<SectionInfo> self, Func<SectionInfo, bool> f)
    {
        var found = false;
        foreach (var x in self)
        {
            if (!found && f(x)) found = true;
            if (found || x.Section.ViewMode is ViewModes.Every or ViewModes.PageFirst)
            {
                yield return x;
            }
        }
    }

    public static IEnumerable<SectionInfo> SkipWhileOrEveryPage(this IEnumerable<SectionInfo> self, Func<SectionInfo, bool> f)
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

    public static IEnumerable<SectionInfo> FooterSort(this IEnumerable<SectionInfo> self)
    {
        var footer = self.ToArray();
        return footer
            .Where(x => x.Section is TotalSection)
            .Reverse()
            .Concat(footer.Where(x => x.Section is FooterSection));
    }

    public static IEnumerable<SectionInfo> PrependIf(this IEnumerable<SectionInfo> self, ISection? section, int level) => section is { } ? self.Prepend(new("", [], section, level)) : self;

    public static IEnumerable<(string[] BreakKeyHierarchy, Section Section, int Level)> AppendHierarchy(this IEnumerable<(Section Section, int Level)> self)
    {
        var keys = new List<string>();
        foreach (var x in self)
        {
            if (x.Section.BreakKey != "") keys.Add(x.Section.BreakKey);
            yield return ([.. keys], x.Section, x.Level);
        }
    }
}

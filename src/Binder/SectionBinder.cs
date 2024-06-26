﻿using Mina.Extension;
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
    public static PageModel[] Bind<T>(PageSection page, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null) => BindPageModels(page, new BufferedEnumerator<T>() { BaseEnumerator = datas.GetEnumerator() }, mapper ?? InstanceMapper.CreateGetMapper<T>())
        .Select(models =>
        {
            // header or detail top-down order
            int top = page.Padding.Top;
            models.Where(x => x.Section is not FooterSection).Each(x => x.Top = (top += x.Section.Height) - x.Section.Height);

            // footer bottom-up order
            int bottom = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - page.Padding.Bottom;
            models.Where(x => x.Section is FooterSection).Reverse().Each(x => x.Top = bottom -= x.Section.Height);

            return new PageModel() { Size = page.Size, Orientation = page.Orientation, Models = models };
        })
        .ToArray();

    public static PageModel[] Bind(PageSection page, DataTable table) => Bind(page,
        table.Rows.GetIterator().OfType<DataRow>(),
        table.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRow, object>>(x => x.ColumnName, x => (row) => row[x]));

    public static PageModel[] Bind(PageSection page, DataView view) => Bind(page,
        view.GetIterator().OfType<DataRowView>(),
        view.Table!.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRowView, object>>(x => x.ColumnName, x => (row) => row[x.ColumnName]));

    public static List<List<SectionModel>> BindPageModels<T>(PageSection page, BufferedEnumerator<T> datas, Dictionary<string, Func<T, object>> mapper)
    {
        var sections = page.SubSection.Travers(x => x is Section s ? [s.SubSection] : []).ToArray();
        var detail = sections.OfType<DetailSection>().First();
        var hierarchy = sections.OfType<Section>().AppendHierarchy().ToArray();
        var headers = hierarchy.Where(x => x.Section.Header is { }).Select(x => (x.Section.BreakKey, x.BreakKeyHierarchy, Section: (ISection)x.Section.Header!)).PrependIf(page.Header).ToArray();
        var footers = hierarchy.Where(x => x.Section.Footer is { }).Select(x => (x.Section.BreakKey, x.BreakKeyHierarchy, Section: (ISection)x.Section.Footer!)).ToArray();
        var keys = sections.OfType<Section>().Select(x => x.BreakKey).Where(x => x.Length > 0).ToArray();

        var pages = new List<List<SectionModel>>();
        var models = new List<SectionModel>();
        var bind = new BindSummaryMapper<T>() { Mapper = mapper };
        bind.CreatePool(page, keys);
        bind.CreateSummaryGoBack(keys.Length);

        var everyfooter = page.Footer is FooterSection footer && footer.ViewMode == ViewModes.Every ? footer : null;
        var pageheight_minus_everypagefooter = PdfUtility.GetPageSize(page.Size, page.Orientation).Height - page.Padding.Top - page.Padding.Bottom - (everyfooter?.Height ?? 0);
        var minimum_breakfooter_height = footers.SkipWhileOrEveryPage(_ => false).Select(x => x.Section.Height).Sum();
        T lastdata = default!;

        while (!datas.IsLast)
        {
            bind.SetPageCount(pages.Count + 1);
            _ = datas.Next(0, out var firstdata);
            if (pages.Count == 0) lastdata = firstdata;
            headers
                .SkipWhileOrPageFirst(x => pages.Count == 0 || (x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](firstdata))))
                .Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, firstdata, bind, page, x.BreakKeyHierarchy, keys) })
                .Each(models.Add);
            var page_first = true;
            var breakcount = 0;

            while (true)
            {
                _ = datas.Next(0, out var current);
                bind.Clear(keys.TakeWhile(x => bind.Mapper[x](lastdata).Equals(bind.Mapper[x](current))).ToArray());
                var breakheader = page_first ? null : headers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](current)));
                var height = pageheight_minus_everypagefooter - (breakheader?.Select(x => x.Section.Height).Sum() ?? 0) - models.Select(x => x.Section.Height).Sum();
                var count = GetBreakOrTakeCount(datas, bind, keys, (height - minimum_breakfooter_height) / detail.Height);
                if (count == 0)
                {
                    if (everyfooter is { }) models.Add(new() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys) });
                    if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys);
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
                        if (everyfooter is { }) models.Add(new() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys) });
                        break;
                    }
                    breakcount = 0;
                    breakfooter = footers.SkipWhileOrEveryPage(_ => false).FooterSort().ToArray();
                }
                breakheader?.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, current, bind, page, x.BreakKeyHierarchy, keys) }).Each(models.Add);

                _ = datas.Next(count - 1, out lastdata);
                datas.GetRange(count).Select(x => new SectionModel() { Section = detail, Elements = BindElements(detail.Elements, x, bind.Return(y => y.DataBind(x)), page, keys, keys) }).Each(models.Add);
                breakfooter.Select(x => new SectionModel() { Section = x.Section, Elements = BindElements(x.Section.Elements, lastdata, bind, page, x.BreakKeyHierarchy, keys) }).Each(models.Add);
                if (breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak))
                {
                    if (everyfooter is { }) models.Add(new() { Section = everyfooter, Elements = BindElements(everyfooter.Elements, lastdata, bind, page, [], keys) });
                    if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys);
                    break;
                }
                if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys);
                page_first = false;

                if (datas.IsLast)
                {
                    if (page.Footer is ISection lastfooter) models.Add(new() { Section = lastfooter, Elements = BindElements(lastfooter.Elements, lastdata, bind, page, [], keys) });
                    break;
                }
            }

            pages.Add([.. models]);
            models.Clear();
            bind.PageBreak(lastdata);
            if (datas.IsLast) bind.LastBreak(lastdata);
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

    public static List<IModelElement> BindElements<T>(List<IElement> elements, T data, BindSummaryMapper<T> bind, PageSection page, string[] keys, string[] allkeys) => elements.Select(x => BindElement(x, data, bind, page, keys, allkeys)).ToList();

    public static IModelElement BindElement<T>(IElement element, T data, BindSummaryMapper<T> bind, PageSection page, string[] keys, string[] allkeys)
    {
        var posx = element.X + page.Padding.Left;
        switch (element)
        {
            case TextElement x: return CreateTextModel(x, x.Text, page);

            case BindElement x: return CreateTextModel(x, BindSummaryMapper<T>.BindFormat(bind.Mapper[x.Bind](data), x.Format), page);

            case SummaryElement x:
                {
                    var keycount = x.BreakKey == "" ? keys.Length - 1 : allkeys.FindLastIndex(y => y == x.BreakKey);
                    var model = CreateTextModel(x, BindSummaryMapper<T>.BindFormat(bind.GetSummary(x, data), x.Format), page);
                    bind.AddSummaryGoBack(x, model, keycount);
                    return model;
                }

            case LineElement x:
                return new LineModel()
                {
                    X = posx,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    Color = x.Color?.ToDeviceRGB(),
                    LineWidth = x.LineWidth,
                };

            case RectangleElement x:
                return new RectangleModel()
                {
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
            foreach (var e in detail.GetSummaryNotIncrement()) yield return (keys, e);
        }
        else if (subsection is Section section)
        {
            var newkeys = section.BreakKey == "" ? keys : [.. keys, section.BreakKey];

            foreach (var e in section.Header?.GetSummaryNotIncrement() ?? []) yield return (newkeys, e);
            foreach (var e in section.Footer?.GetSummaryNotIncrement() ?? []) yield return (newkeys, e);

            if (section.SubSection is { })
            {
                foreach (var x in GetBreakKeyWithSummary(newkeys, section.SubSection)) yield return x;
            }
        }
    }

    public static IEnumerable<SummaryElement> GetSummaryNotIncrement(this ISection section) => section.Elements.OfType<SummaryElement>().Where(x => x.SummaryMethod != SummaryMethod.Increment);

    public static IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> SkipWhileOrPageFirst(this IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> self, Func<(string BreakKey, string[] BreakKeyHierarchy, ISection Section), bool> f)
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

    public static IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> SkipWhileOrEveryPage(this IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> self, Func<(string BreakKey, string[] BreakKeyHierarchy, ISection Section), bool> f)
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

    public static IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> FooterSort(this IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> self)
    {
        var footer = self.ToArray();
        return footer
            .Where(x => x.Section is TotalSection)
            .Reverse()
            .Concat(footer.Where(x => x.Section is FooterSection));
    }

    public static IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> PrependIf(this IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)> self, ISection? section) => section is { } ? (IEnumerable<(string BreakKey, string[] BreakKeyHierarchy, ISection Section)>)self.Prepend(("", [], section)) : self;

    public static IEnumerable<(string[] BreakKeyHierarchy, Section Section)> AppendHierarchy(this IEnumerable<Section> self)
    {
        var keys = new List<string>();
        foreach (var x in self)
        {
            if (x.BreakKey != "") keys.Add(x.BreakKey);
            yield return ([.. keys], x);
        }
    }
}

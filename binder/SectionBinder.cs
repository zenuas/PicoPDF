using Binder.Data;
using Binder.Model;
using Mina.Extension;
using Mina.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Binder;

public static class SectionBinder
{
    public static TPage[] Bind<T, TPage, TSection>(IPageSection<TSection> page, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null)
        where TPage : IPageModel<TPage, TSection>
        where TSection : ISectionModel<TSection>
        => [.. BindPageModels<T, TSection>(page, new BufferedEnumerator<T>() { BaseEnumerator = datas.GetEnumerator() }, mapper ?? InstanceMapper.CreateGetMapper<T>())
        .Select(models =>
        {
            // header or detail top-down order
            int top = page.Padding.Top;
            models.Where(x => !x.IsFooter).Each(x => x.Top = (top += x.Height) - x.Height);

            // footer bottom-up order
            var height = page.Height;
            int bottom = height - page.Padding.Bottom;
            models.Where(x => x.IsFooter).Reverse().Each(x => x.Top = bottom -= x.Height);
            
            // cross section update position
            models.Each(x => x.UpdatePosition());

            return TPage.CreatePageModel(page.Width, height, models);
        })];

    public static TPage[] Bind<TPage, TSection>(IPageSection<TSection> page, DataTable table)
        where TSection : ISectionModel<TSection>
        where TPage : IPageModel<TPage, TSection>
        => Bind<DataRow, TPage, TSection>(
            page,
            table.Rows.GetIterator().OfType<DataRow>(),
            table.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRow, object>>(x => x.ColumnName, x => (row) => row?[x]!));

    public static TPage[] Bind<TPage, TSection>(IPageSection<TSection> page, DataView view)
        where TSection : ISectionModel<TSection>
        where TPage : IPageModel<TPage, TSection>
        => Bind<DataRowView, TPage, TSection>(
            page,
            view.GetIterator().OfType<DataRowView>(),
            view.Table!.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRowView, object>>(x => x.ColumnName, x => (row) => row?[x.ColumnName]!));

    public static (SectionInfo[] Headers, SectionInfo[] Footers, IDetailSection Detail, string[] BreakKeys) GetSectionInfo(ISubSection subsection, IHeaderSection? pageheader)
    {
        var sections = (Section: subsection, Depth: 1, BreakKey: (subsection as IBreakKey)?.BreakKey ?? "").Travers(x => x.Section is IParentSection s ? [(s.SubSection, x.Depth + 1, (s.SubSection as IBreakKey)?.BreakKey ?? "")] : []).ToArray();
        var detail = sections.Select(x => x.Section).OfType<IDetailSection>().First();
        var break_count = 0;
        var hierarchy = sections.Where(x => x.Section is IParentSection).Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, Section: x.Section.Cast<IParentSection>(), x.Depth, x.BreakKey)).ToArray();
        var headers = hierarchy.Where(x => x.Section.Header is { }).Select(x => new SectionInfo(x.BreakKey, x.BreakCount, x.Section.Header!, x.Depth)).To(x => pageheader is { } ? x.Prepend(new("", 0, pageheader, 0)) : x);
        var footers = hierarchy.Where(x => x.Section.Footer is { }).Select(x => new SectionInfo(x.BreakKey, x.BreakCount, x.Section.Footer!, x.Depth));
        var keys = sections.Where(x => x.BreakKey != "").Select(x => x.BreakKey);

        return ([.. headers], [.. footers], detail, [.. keys]);
    }

    public static bool CompareBreakKey<T, TSection>(BindSummaryMapper<T, TSection> bind, string break_key, T data1, T data2)
        where TSection : ISectionModel<TSection>
        => bind.Mapper[break_key](data1).Equals(bind.Mapper[break_key](data2));

    public static TSection ReturnBreakSection<T, TSection>(this BindSummaryMapper<T, TSection> bind, TSection section)
        where TSection : ISectionModel<TSection>
    {
        bind.BreakSection(section);
        return section;
    }

    public static IEnumerable<TSection[]> BindPageModels<T, TSection>(IPageSection<TSection> page, BufferedEnumerator<T> datas, Dictionary<string, Func<T, object>> mapper)
        where TSection : ISectionModel<TSection>
    {
        var (headers, footers, detail, keys) = GetSectionInfo(page.SubSection, page.Header);
        var bind = BindSummaryMapper<T, TSection>.Create(page, mapper, keys, headers.LastOrDefault()?.Depth ?? 0);

        if (datas.IsLast)
        {
            T nodata = default!;
            var models = new List<TSection>();
            bind.SetPageCount(1);
            headers
                .Select(x => page.BindSection(TSection.CreateSectionModel(page, x.Section, nodata, bind, x.BreakCount, x.Depth)))
                .Each(models.Add);
            footers
                .FooterSort()
                .Select(x => page.BindSection(TSection.CreateSectionModel(page, x.Section, nodata, bind, x.BreakCount, x.Depth)))
                .Select(bind.ReturnBreakSection)
                .Each(models.Add);
            if (page.Footer is ISection lastfooter) models.Add(bind.ReturnBreakSection(page.BindSection(TSection.CreateSectionModel(page, lastfooter, nodata, bind, 0, null))));
            bind.KeyBreak(nodata, keys.Length, keys, page);
            bind.PageBreak(nodata, page);
            bind.LastBreak(nodata, page);
            yield return [.. models];
            yield break;
        }

        var page_count = 0;
        var everyfooter = page.Footer is { } footer && footer.IsFooter && footer.ViewMode == ViewModes.Every ? footer : null;
        var pageheight_minus_everypagefooter = page.Height - page.Padding.Top - page.Padding.Bottom - (everyfooter?.Height ?? 0);
        var minimum_breakfooter_height = footers.SkipWhileOrEveryPage(_ => false).Select(x => x.Section.Height).Sum();
        T lastdata = default!;
        TSection lastdetail = default!;
        while (!datas.IsLast)
        {
            var models = new List<TSection>();
            bind.SetPageCount(++page_count);
            _ = datas.Next(0, out var firstdata);
            if (page_count == 1) lastdata = firstdata;
            headers
                .SkipWhileOrPageFirst(x => page_count == 1 || (x.BreakKey != "" && !CompareBreakKey(bind, x.BreakKey, lastdata, firstdata)))
                .Select(x => page.BindSection(TSection.CreateSectionModel(page, x.Section, firstdata, bind, x.BreakCount, x.Depth)))
                .Where(x => x.IsVisible)
                .Each(models.Add);
            var page_first = true;

            while (true)
            {
                _ = datas.Next(0, out var current);
                var breakheader_section = page_first ?
                    null :
                    headers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !CompareBreakKey(bind, x.BreakKey, lastdata, current))
                        ?.Select(x => page.BindSection(TSection.CreateSectionModel(page, x.Section, current, bind, x.BreakCount, x.Depth)))
                        .Where(x => x.IsVisible)
                        .ToArray();
                var height = pageheight_minus_everypagefooter - (breakheader_section?.Select(x => x.Height).Sum() ?? 0) - models.Select(x => x.Height).Sum();

                var temp_everyfooter = everyfooter is null || page_first ? default : page.BindSection(TSection.CreateSectionModel(page, everyfooter, lastdata, bind, 0, null));
                if (!page_first) bind.SectionBreak(lastdata, page);

                var details = GetBreakOrTakeDetail(page, detail, datas, bind, keys, height - minimum_breakfooter_height);
                if (details.Count == 0)
                {
                    if (temp_everyfooter is { } && temp_everyfooter.IsVisible) models.Add(bind.ReturnBreakSection(temp_everyfooter));
                    break;
                }

                var existnext = datas.Next(0, out var next);
                var breakcount = keys.Length - (existnext ? keys.TakeWhile(x => CompareBreakKey(bind, x, current, next)).Count() : 0);
                var breakfooter = existnext ? [.. footers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !CompareBreakKey(bind, x.BreakKey, current, next))] : footers;
                if (height < details.Select(x => x.Section.Height).Sum() + breakfooter.Select(x => x.Section.Height).Sum())
                {
                    var last = details[^1].Data;
                    bind.DataBindCancel(last);
                    datas.PushBack(last);
                    details.RemoveAt(details.Count - 1);
                    if (details.Count <= 0)
                    {
                        if (temp_everyfooter is { } && temp_everyfooter.IsVisible) models.Add(bind.ReturnBreakSection(temp_everyfooter));
                        break;
                    }
                    breakcount = 0;
                    breakfooter = [.. footers.SkipWhileOrEveryPage(_ => false)];
                }

                breakheader_section?.Each(models.Add);
                details
                    .Select(x => x.Section)
                    .Where(x => x.IsVisible)
                    .Each(models.Add);
                lastdata = details[^1].Data;
                var breakfooter_section = breakfooter
                    .FooterSort()
                    .Select(x => page.BindSection(TSection.CreateSectionModel(page, x.Section, lastdata, bind, x.BreakCount, x.Depth)))
                    .Select(bind.ReturnBreakSection)
                    .ToArray();

                if (breakcount > 0 && detail.Fill)
                {
                    var fillarea = height - details.Select(x => x.Section.Height).Sum() - breakfooter_section.Where(x => x.IsVisible).Select(x => x.Height).Sum();
                    while (fillarea > 0)
                    {
                        var fill = page.BindSection(TSection.CreateSectionModel(page, detail, default!, bind.Empty, keys.Length, null));
                        if (fill.Height > fillarea || fill.Height <= 0 || !fill.IsVisible) break;
                        models.Add(fill.Cast<TSection>());
                        fillarea -= fill.Height;
                    }
                }
                lastdetail = models.Last();
                breakfooter_section
                    .Where(x => x.IsVisible)
                    .Each(models.Add);
                if (breakfooter_section.Contains(x => x.IsPageBreak))
                {
                    if (everyfooter is { })
                    {
                        var pagebreak_footer = page.BindSection(TSection.CreateSectionModel(page, everyfooter, lastdata, bind, 0, null));
                        if (pagebreak_footer.IsVisible) models.Add(bind.ReturnBreakSection(pagebreak_footer));
                    }
                    if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys, page);
                    break;
                }
                if (datas.IsLast && page.Footer is ISection lastfooter)
                {
                    var pagelast_footer = page.BindSection(TSection.CreateSectionModel(page, lastfooter, lastdata, bind, 0, null));
                    if (pagelast_footer.IsVisible) models.Add(pagelast_footer.Cast<TSection>());
                }
                if (datas.IsLast || breakcount > 0)
                {
                    bind.KeyBreak(lastdata, breakcount, keys, page);
                    if (datas.IsLast) break;
                }
                page_first = false;
            }

            bind.PageBreak(lastdata, page);
            bind.PageBreakSection(lastdetail);
            if (datas.IsLast) bind.LastBreak(lastdata, page);
            yield return [.. models];
        }
    }

    public static List<(TSection Section, T Data)> GetBreakOrTakeDetail<T, TSection>(IPageSection<TSection> page, IDetailSection detail, BufferedEnumerator<T> datas, BindSummaryMapper<T, TSection> bind, string[] keys, int max_height)
        where TSection : ISectionModel<TSection>
    {
        if (max_height <= 0) return [];

        var details = new List<(TSection Section, T Data)>();
        _ = datas.Next(0, out var prev);
        var prevkey = keys.ToDictionary(x => x, x => bind.Mapper[x](prev));
        bind.DataBind(prev);
        var first = page.BindSection(TSection.CreateSectionModel(page, detail, prev, bind, 0, null)).Cast<TSection>();
        var height = first.Height;
        details.Add((first, prev));
        _ = datas.Pop();

        while (true)
        {
            if (!datas.Next(0, out var data) ||
                !keys.All(x => prevkey[x].Equals(bind.Mapper[x](data)))) break;

            bind.DataBind(data);
            var next = page.BindSection(TSection.CreateSectionModel(page, detail, data, bind, 0, null)).Cast<TSection>();
            height += next.Height;
            if (height > max_height)
            {
                bind.DataBindCancel(data);
                break;
            }

            _ = datas.Pop();
            details.Add((next, data));
        }
        return details;
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
            .Where(x => !x.Section.Cast<IFooterSection>().IsFooter)
            .Reverse()
            .Concat(footer.Where(x => x.Section.Cast<IFooterSection>().IsFooter));
    }
}

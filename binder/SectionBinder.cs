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
    public static R[] Bind<T, M, R>(IPageSection page, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null)
        where M : ISectionModel<M>
        where R : IPageModel<R, M>
        => [.. BindPageModels<T, M>(page, new BufferedEnumerator<T>() { BaseEnumerator = datas.GetEnumerator() }, mapper ?? InstanceMapper.CreateGetMapper<T>())
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

            return R.CreatePageModel(page.Width, height, models);
        })];

    public static R[] Bind<M, R>(IPageSection page, DataTable table)
        where M : ISectionModel<M>
        where R : IPageModel<R, M>
        => Bind<DataRow, M, R>(
            page,
            table.Rows.GetIterator().OfType<DataRow>(),
            table.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRow, object>>(x => x.ColumnName, x => (row) => row?[x]!));

    public static R[] Bind<M, R>(IPageSection page, DataView view)
        where M : ISectionModel<M>
        where R : IPageModel<R, M>
        => Bind<DataRowView, M, R>(
            page,
            view.GetIterator().OfType<DataRowView>(),
            view.Table!.Columns.GetIterator().OfType<DataColumn>().ToDictionary<DataColumn, string, Func<DataRowView, object>>(x => x.ColumnName, x => (row) => row?[x.ColumnName]!));

    public static (
        SectionInfo[] Headers,
        SectionInfo[] FootersWithoutPageFooter,
        IDetailSection Detail,
        string[] BreakKeys)
        GetSectionInfo(ISubSection subsection, IHeaderSection? pageheader)
    {
        var sections = (Section: subsection, Depth: 1, BreakKey: (subsection as IBreakKey)?.BreakKey ?? "").Travers(x => x.Section is IParentSection s ? [(s.SubSection, x.Depth + 1, (s.SubSection as IBreakKey)?.BreakKey ?? "")] : []).ToArray();
        var detail = sections.Select(x => x.Section).OfType<IDetailSection>().First();
        var break_count = 0;
        var hierarchy = sections.Where(x => x.Section is IParentSection).Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, Section: x.Section.Cast<IParentSection>(), x.Depth, x.BreakKey)).ToArray();
        var headers = hierarchy.Where(x => x.Section.Header is { }).Select(x => new SectionInfo(x.BreakKey, x.BreakCount, x.Section.Header!, x.Depth)).To(x => pageheader is { } ? x.Prepend(new("", 0, pageheader, 0)) : x);
        var footers = hierarchy.Where(x => x.Section.Footer is { }).Select(x => new SectionInfo(x.BreakKey, x.BreakCount, x.Section.Footer!, x.Depth));
        var keys = sections.Select(x => x.BreakKey).Where(x => x.Length > 0);

        return ([.. headers], [.. footers], detail, [.. keys]);
    }

    public static IEnumerable<M[]> BindPageModels<T, M>(IPageSection page, BufferedEnumerator<T> datas, Dictionary<string, Func<T, object>> mapper)
        where M : ISectionModel<M>
    {
        var (headers, footers, detail, keys) = GetSectionInfo(page.SubSection, page.Header);

        var bind = new BindSummaryMapper<M, T>() { Mapper = mapper, Keys = keys };
        bind.CreatePool(page);
        bind.CreateSummaryGoBack();
        bind.CreateCrossSectionGoBack(headers.LastOrDefault()?.Depth ?? 0);

        if (datas.IsLast)
        {
            T nodata = default!;
            var models = new List<M>();
            bind.SetPageCount(1);
            headers.Select(x => M.CreateSectionModel(page, x.Section, nodata, bind, x.BreakCount, x.Depth)).Each(models.Add);
            footers.FooterSort().Select(x => M.CreateSectionModel(page, x.Section, nodata, bind, x.BreakCount, x.Depth).Return(x => bind.BreakSection(x))).Each(models.Add);
            if (page.Footer is ISection lastfooter) models.Add(M.CreateSectionModel(page, lastfooter, nodata, bind, 0, null).Return(x => bind.BreakSection(x)));
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
        M lastdetail = default!;
        while (!datas.IsLast)
        {
            var models = new List<M>();
            bind.SetPageCount(++page_count);
            _ = datas.Next(0, out var firstdata);
            if (page_count == 1) lastdata = firstdata;
            headers
                .SkipWhileOrPageFirst(x => page_count == 1 || (x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](firstdata))))
                .Select(x => M.CreateSectionModel(page, x.Section, firstdata, bind, x.BreakCount, x.Depth))
                .Each(models.Add);
            var page_first = true;
            var breakcount = 0;

            while (true)
            {
                _ = datas.Next(0, out var current);
                var breakheader = page_first ? null : headers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](lastdata).Equals(bind.Mapper[x.BreakKey](current)));
                var height = pageheight_minus_everypagefooter - (breakheader?.Select(x => x.Section.Height).Sum() ?? 0) - models.Select(x => x.Height).Sum();
                var count = GetBreakOrTakeCount(datas, bind, keys, (height - minimum_breakfooter_height) / detail.Height);
                if (count == 0)
                {
                    if (everyfooter is { }) models.Add(M.CreateSectionModel(page, everyfooter, lastdata, bind, 0, null).Return(x => bind.BreakSection(x)));
                    break;
                }

                var existnext = datas.Next(count, out var next);
                breakcount = keys.Length - (existnext ? keys.TakeWhile(x => bind.Mapper[x](current).Equals(bind.Mapper[x](next))).Count() : 0);
                var breakfooter = (existnext ? [.. footers.SkipWhileOrEveryPage(x => x.BreakKey != "" && !bind.Mapper[x.BreakKey](current).Equals(bind.Mapper[x.BreakKey](next)))] : footers);

                if (height < (count * detail.Height) + breakfooter.Select(x => x.Section.Height).Sum())
                {
                    if (--count <= 0)
                    {
                        if (everyfooter is { }) models.Add(M.CreateSectionModel(page, everyfooter, lastdata, bind, 0, null).Return(x => bind.BreakSection(x)));
                        break;
                    }
                    breakcount = 0;
                    breakfooter = [.. footers.SkipWhileOrEveryPage(_ => false)];
                }
                if (!page_first) bind.SectionBreak(lastdata, page);
                breakheader?.Select(x => M.CreateSectionModel(page, x.Section, current, bind, x.BreakCount, x.Depth)).Each(models.Add);

                _ = datas.Next(count - 1, out lastdata);
                foreach (var x in datas.GetRange(count))
                {
                    bind.DataBind(x);
                    models.Add(M.CreateSectionModel(page, detail, x, bind, keys.Length, null));
                }
                lastdetail = models.Last();
                breakfooter.FooterSort().Select(x => M.CreateSectionModel(page, x.Section, lastdata, bind, x.BreakCount, x.Depth).Return(x => bind.BreakSection(x))).Each(models.Add);
                if (breakfooter.Contains(x => x.Section.Cast<IFooterSection>().PageBreak))
                {
                    if (everyfooter is { }) models.Add(M.CreateSectionModel(page, everyfooter, lastdata, bind, 0, null).Return(x => bind.BreakSection(x)));
                    if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys, page);
                    break;
                }
                if (breakcount > 0) bind.KeyBreak(lastdata, breakcount, keys, page);
                page_first = false;

                if (datas.IsLast)
                {
                    if (page.Footer is ISection lastfooter) models.Add(M.CreateSectionModel(page, lastfooter, lastdata, bind, 0, null));
                    break;
                }
            }

            bind.PageBreak(lastdata, page);
            bind.PageBreakSection(lastdetail);
            if (datas.IsLast) bind.LastBreak(lastdata, page);
            yield return [.. models];
        }
    }

    public static int GetBreakOrTakeCount<M, T>(BufferedEnumerator<T> datas, BindSummaryMapper<M, T> bind, string[] keys, int maxcount)
        where M : ISectionModel<M>
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

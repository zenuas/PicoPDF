using Mina.Extension;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Binder;

public class BindSummaryMapper<T>
{
    public required Dictionary<string, Func<T, object>> Mapper { get; init; }
    public Dictionary<string, ClearableDynamicValue> SummaryPool { get; init; } = [];
    public List<Action<T>> SummaryAction { get; init; } = [];
    public List<List<(SummaryElement SummaryElement, MutableTextModel TextModel)>> SummaryGoBack { get; init; } = [];
    public List<List<ICrossSectionModel>> CrossSectionGoBack { get; init; } = [];

    public void CreatePool(PageSection page, string[] allkeys)
    {
        SummaryPool.Add("#:PAGECOUNT()", new() { Value = 1, Clear = _ => { } });
        TraversSummaryElement([], page).Each(sr =>
        {
            var keys = sr.SummaryElement.BreakKey == "" ? sr.BreakKeys : [.. allkeys.Take(allkeys.FindLastIndex(x => x == sr.SummaryElement.BreakKey) + 1)];
            var breakpoint =
                sr.SummaryElement.SummaryMethod is SummaryMethod.All or SummaryMethod.AllIncremental ? "#" :
                sr.SummaryElement.SummaryMethod is SummaryMethod.Page or SummaryMethod.PageIncremental ? $"&{keys.Join(".")}" :
                sr.SummaryElement.SummaryMethod is SummaryMethod.CrossSectionPage or SummaryMethod.CrossSectionPageIncremental ? $"%{keys.Join(".")}" :
                $"${keys.Join(".")}";
            switch (sr.SummaryElement.SummaryType)
            {
                case SummaryType.Summary:
                case SummaryType.Average:
                    {
                        var key = $"{breakpoint}:{sr.SummaryElement.Bind}";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = 0, Clear = x => x.Value = 0 };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value);
                            SummaryAction.Add(x => v.Value += (dynamic)Mapper[sr.SummaryElement.Bind](x));
                        }
                        sr.SummaryElement.SummaryBind = key;
                        if (sr.SummaryElement.SummaryType == SummaryType.Average) goto case SummaryType.Count;
                    }
                    break;

                case SummaryType.Count:
                    {
                        var key = $"{breakpoint}:COUNT()";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = 0, Clear = x => x.Value = 0 };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value);
                            SummaryAction.Add(x => v.Value++);
                        }
                        sr.SummaryElement.SummaryCount = key;
                    }
                    break;

                case SummaryType.Maximum:
                    {
                        var key = $"{breakpoint}:MAX({sr.SummaryElement.Bind})";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => x.Value = null };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value!);
                            SummaryAction.Add(x =>
                            {
                                var value = (dynamic)Mapper[sr.SummaryElement.Bind](x);
                                v.Value = v.Value is null ? value : (v.Value < value ? value : v.Value);
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                    }
                    break;

                case SummaryType.Minimum:
                    {
                        var key = $"{breakpoint}:MIN({sr.SummaryElement.Bind})";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => x.Value = null };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value!);
                            SummaryAction.Add(x =>
                            {
                                var value = (dynamic)Mapper[sr.SummaryElement.Bind](x);
                                v.Value = v.Value is null ? value : (v.Value > value ? value : v.Value);
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                    }
                    break;
            }
        });
    }

    public void CreateSummaryGoBack(int hierarchy_count) => Lists.RangeTo(0, hierarchy_count + 2).Each(_ => SummaryGoBack.Add([]));

    public void CreateCrossSectionGoBack(int hierarchy_count) => Lists.RangeTo(0, hierarchy_count).Each(_ => CrossSectionGoBack.Add([]));

    public void AddSummaryGoBack(SummaryElement summary, MutableTextModel model, int hierarchy_count)
    {
        switch (summary.SummaryMethod)
        {
            case SummaryMethod.All:
                SummaryGoBack[0].Add((summary, model));
                break;

            case SummaryMethod.Page:
                SummaryGoBack[1].Add((summary, model));
                break;

            case SummaryMethod.CrossSectionPage:
                SummaryGoBack[2].Add((summary, model));
                break;

            case SummaryMethod.Group:
                SummaryGoBack[hierarchy_count + 3].Add((summary, model));
                break;
        }
    }

    public void AddCrossSectionGoBack(ICrossSectionModel model, int depth) => CrossSectionGoBack[depth].Add(model);

    public void SectionBreak(T data, PageSection page)
    {
        SummaryGoBack[1].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[1].Clear();
        SummaryPool.Keys.Where(x => x.StartsWith('&')).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void PageBreak(T data, PageSection page)
    {
        SectionBreak(data, page);

        SummaryGoBack[2].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[2].Clear();
        SummaryPool.Keys.Where(x => x.StartsWith('%')).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void KeyBreak(T data, int hierarchy_count, string[] allkeys, PageSection page)
    {
        for (int i = SummaryGoBack.Count - hierarchy_count; i < SummaryGoBack.Count; i++)
        {
            SummaryGoBack[i].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
            SummaryGoBack[i].Clear();
        }

        var nobreak = allkeys.Take(allkeys.Length - hierarchy_count + 1).Join(".");
        var sumkey_via_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
        var sumkey_direct_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}:" : nobreak)}";
        SummaryPool.Keys.Where(x => x.StartsWith(sumkey_via_prefix) || x.StartsWith(sumkey_direct_prefix)).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void LastBreak(T data, PageSection page)
    {
        SummaryGoBack[0].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[0].Clear();
    }

    public void BreakSection(SectionModel model)
    {
        var depth = model.Depth ?? throw new();
        CrossSectionGoBack.Take(depth + 1).Flatten().Each(x => x.TargetModel = model);
        CrossSectionGoBack[depth].Clear();
    }

    public void PageBreakSection(SectionModel model)
    {
        for (var i = 0; i < CrossSectionGoBack.Count; i++)
        {
            CrossSectionGoBack[i].Each(x => x.TargetModel ??= model);
            CrossSectionGoBack[i].Clear();
        }
    }

    public static string BindFormat(object? o, string format, IFormatProvider provider) => (format != "" && o is IFormattable formattable ? formattable.ToString(format, provider) : o?.ToString()) ?? "";

    public static string BindFormat(object? o, string format, IFormatProvider provider, object nan)
    {
        try
        {
            return BindFormat(o, format, provider);
        }
        catch
        {
            return BindFormat(nan, format, provider);
        }
    }

    public void SetPageCount(int pagecount) => SummaryPool["#:PAGECOUNT()"].Value = pagecount;

    public void DataBind(T data) => SummaryAction.Each(x => x(data));

    public object GetSummary(SummaryElement x, T data)
    {
        switch (x.SummaryType)
        {
            case SummaryType.Summary: return Mapper[x.SummaryBind](data);
            case SummaryType.Count: return (int)Mapper[x.SummaryCount](data);

            case SummaryType.Average:
                dynamic count = Mapper[x.SummaryCount](data);
                if (count == 0) return x.NaN;
                return ((dynamic)Mapper[x.SummaryBind](data)) / count;

            case SummaryType.Maximum or SummaryType.Minimum: return Mapper[x.SummaryBind](data);
            case SummaryType.PageCount: return SummaryPool["#:PAGECOUNT()"].Value!;
        }
        throw new();
    }

    public static IEnumerable<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, IParentSection section)
    {
        if (section is Section sec && sec.BreakKey != "") keys = [.. keys, sec.BreakKey];

        if (section.Header is ISection header) foreach (var x in TraversSummaryElement(keys, header.Elements)) yield return x;
        if (section.SubSection is ISection detail) foreach (var x in TraversSummaryElement(keys, detail.Elements)) yield return x;
        if (section.SubSection is IParentSection subsection) foreach (var x in TraversSummaryElement(keys, subsection)) yield return x;
        if (section.Footer is ISection footer) foreach (var x in TraversSummaryElement(keys, footer.Elements)) yield return x;
    }

    public static IEnumerable<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, List<IElement> elements) => elements.OfType<SummaryElement>().Select(x => (keys, x));
}

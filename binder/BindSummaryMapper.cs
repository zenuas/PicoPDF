using Binder.Data;
using Binder.Model;
using Mina.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Binder;

public class BindSummaryMapper<T, TSection>
    where TSection : ISectionModel<TSection>
{
    public required IReadOnlyDictionary<string, Func<T, object>> Mapper { get; init; }
    public string[] Keys { get; init; } = [];
    public required IReadOnlyDictionary<string, ClearableDynamicValue> SummaryPool { get; init; }
    public Action<T>[] SummaryAction { get; init; } = [];
    public Action<T>[] SummaryCancelAction { get; init; } = [];
    public required List<(ISummaryElement SummaryElement, IMutableTextModel TextModel)>[] SummaryGoBack { get; init; }
    public required List<ICrossSectionModel<TSection>>[] CrossSectionGoBack { get; init; }
    public required Func<int> GetPageCount { get; init; }
    public bool IsEmpty { get; init; } = false;

    public BindSummaryMapper<T, TSection> Empty { get; init; } = EmptyInstance;

    public static readonly BindSummaryMapper<T, TSection> EmptyInstance = new()
    {
        Mapper = NullMapper<T>.Instance,
        Keys = [],
        SummaryPool = new Dictionary<string, ClearableDynamicValue>(),
        SummaryAction = [],
        SummaryCancelAction = [],
        SummaryGoBack = CreateSummaryGoBack(0),
        CrossSectionGoBack = CreateCrossSectionGoBack(0),
        GetPageCount = () => 0,
        IsEmpty = true,
    };

    public static BindSummaryMapper<T, TSection> Create(IPageSection<TSection> page, Dictionary<string, Func<T, object>> mapper, string[] keys, int depth)
    {
        var (pool, actions, cancels) = CreatePool(page, mapper, keys);
        return new()
        {
            Mapper = mapper,
            Keys = keys,
            SummaryPool = pool,
            SummaryAction = actions,
            SummaryCancelAction = cancels,
            SummaryGoBack = CreateSummaryGoBack(keys.Length),
            CrossSectionGoBack = CreateCrossSectionGoBack(depth),
            GetPageCount = () => (int)pool["#:PAGECOUNT()"].Value!,
            IsEmpty = false,

            Empty = new()
            {
                Mapper = NullMapper<T>.Instance,
                Keys = [],
                SummaryPool = new Dictionary<string, ClearableDynamicValue>(),
                SummaryAction = [],
                SummaryCancelAction = [],
                SummaryGoBack = CreateSummaryGoBack(0),
                CrossSectionGoBack = CreateCrossSectionGoBack(0),
                GetPageCount = () => (int)pool["#:PAGECOUNT()"].Value!,
                IsEmpty = true,
            },
        };
    }

    public static (Dictionary<string, ClearableDynamicValue> Pool, Action<T>[] Actions, Action<T>[] Cancels) CreatePool(IPageSection<TSection> page, Dictionary<string, Func<T, object>> mapper, string[] allkeys)
    {
        var pool = new Dictionary<string, ClearableDynamicValue>();
        var actions = new List<Action<T>>();
        var cancels = new List<Action<T>>();

        pool.Add("#:PAGECOUNT()", new() { Value = 1, Clear = _ => { } });
        TraversSummaryElement([], page).Each(sr =>
        {
            var keys = sr.SummaryElement.BreakKey == "" ? sr.BreakKeys : [.. allkeys.Take(allkeys.FindLastIndex(x => x == sr.SummaryElement.BreakKey) + 1)];
            var breakpoint =
                sr.SummaryElement.SummaryMethod is SummaryMethods.All or SummaryMethods.AllIncremental ? "#" :
                sr.SummaryElement.SummaryMethod is SummaryMethods.Page or SummaryMethods.PageIncremental ? "&" :
                sr.SummaryElement.SummaryMethod is SummaryMethods.CrossSectionPage or SummaryMethods.CrossSectionPageIncremental ? "%" :
                $"${keys.Join(".")}";
            switch (sr.SummaryElement.SummaryType)
            {
                case SummaryTypes.Summary:
                case SummaryTypes.Average:
                    {
                        var key = $"{breakpoint}:{sr.SummaryElement.Bind}";
                        if (!pool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = 0, Clear = x => x.Value = 0 };
                            pool.Add(key, v);
                            mapper.Add(key, _ => v.Value);
                            actions.Add(x => v.Value += (dynamic)mapper[sr.SummaryElement.Bind](x));
                            cancels.Add(x => v.Value -= (dynamic)mapper[sr.SummaryElement.Bind](x));
                        }
                        sr.SummaryElement.SummaryBind = key;
                        if (sr.SummaryElement.SummaryType == SummaryTypes.Average) goto case SummaryTypes.Count;
                        break;
                    }

                case SummaryTypes.Count:
                    {
                        var key = $"{breakpoint}:COUNT()";
                        if (!pool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = 0, Clear = x => x.Value = 0 };
                            pool.Add(key, v);
                            mapper.Add(key, _ => v.Value);
                            actions.Add(x => v.Value++);
                            cancels.Add(x => v.Value--);
                        }
                        sr.SummaryElement.SummaryCount = key;
                        break;
                    }

                case SummaryTypes.Maximum:
                    {
                        var key = $"{breakpoint}:MAX({sr.SummaryElement.Bind})";
                        if (!pool.ContainsKey(key))
                        {
                            var values = new List<dynamic>();
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => { x.Value = null; values.Clear(); } };
                            pool.Add(key, v);
                            mapper.Add(key, _ => v.Value!);
                            actions.Add(x =>
                            {
                                var value = (dynamic)mapper[sr.SummaryElement.Bind](x);
                                v.Value = v.Value is null ? value : (v.Value < value ? value : v.Value);
                                values.Add(value);
                            });
                            cancels.Add(_ =>
                            {
                                values.RemoveAt(values.Count - 1);
                                v.Value = values.Count > 0 ? values.Max() : null;
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                        break;
                    }

                case SummaryTypes.Minimum:
                    {
                        var key = $"{breakpoint}:MIN({sr.SummaryElement.Bind})";
                        if (!pool.ContainsKey(key))
                        {
                            var values = new List<dynamic>();
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => { x.Value = null; values.Clear(); } };
                            pool.Add(key, v);
                            mapper.Add(key, _ => v.Value!);
                            actions.Add(x =>
                            {
                                var value = (dynamic)mapper[sr.SummaryElement.Bind](x);
                                v.Value = v.Value is null ? value : (v.Value > value ? value : v.Value);
                                values.Add(value);
                            });
                            cancels.Add(_ =>
                            {
                                values.RemoveAt(values.Count - 1);
                                v.Value = values.Count > 0 ? values.Min() : null;
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                        break;
                    }
            }
        });
        return (pool, [.. actions], [.. cancels]);
    }

    public static List<(ISummaryElement SummaryElement, IMutableTextModel TextModel)>[] CreateSummaryGoBack(int key_count) => [.. Lists.RangeTo(0, key_count + 2).Select(_ => new List<(ISummaryElement SummaryElement, IMutableTextModel TextModel)>())];

    public static List<ICrossSectionModel<TSection>>[] CreateCrossSectionGoBack(int depth) => [.. Lists.RangeTo(0, depth).Select(_ => new List<ICrossSectionModel<TSection>>())];

    public void AddSummaryGoBack(ISummaryElement summary, IMutableTextModel model, int break_count)
    {
        switch (summary.SummaryMethod)
        {
            case SummaryMethods.All:
                SummaryGoBack[0].Add((summary, model));
                break;

            case SummaryMethods.Page:
                SummaryGoBack[1].Add((summary, model));
                break;

            case SummaryMethods.CrossSectionPage:
                SummaryGoBack[2].Add((summary, model));
                break;

            case SummaryMethods.Group:
                var hierarchy_count = summary.BreakKey == "" ? break_count - 1 : Keys.FindLastIndex(y => y == summary.BreakKey);
                SummaryGoBack[hierarchy_count + 3].Add((summary, model));
                break;
        }
    }

    public void AddCrossSectionGoBack(ICrossSectionModel<TSection> model, int depth) => CrossSectionGoBack[depth].Add(model);

    public void SectionBreak(T data, IPageSection<TSection> page)
    {
        SummaryGoBack[1].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[1].Clear();
        SummaryPool.Keys.Where(x => x.StartsWith('&')).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void PageBreak(T data, IPageSection<TSection> page)
    {
        SectionBreak(data, page);

        SummaryGoBack[2].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[2].Clear();
        SummaryPool.Keys.Where(x => x.StartsWith('%')).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void KeyBreak(T data, int hierarchy_count, string[] allkeys, IPageSection<TSection> page)
    {
        for (var i = SummaryGoBack.Length - hierarchy_count; i < SummaryGoBack.Length; i++)
        {
            SummaryGoBack[i].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
            SummaryGoBack[i].Clear();
        }

        var nobreak = allkeys.Take(allkeys.Length - hierarchy_count + 1).Join(".");
        var sumkey_via_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
        var sumkey_direct_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}:" : nobreak)}";
        SummaryPool.Keys.Where(x => x.StartsWith(sumkey_via_prefix) || x.StartsWith(sumkey_direct_prefix)).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void LastBreak(T data, IPageSection<TSection> page)
    {
        SummaryGoBack[0].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format, x.SummaryElement.Culture ?? page.DefaultCulture, x.SummaryElement.NaN));
        SummaryGoBack[0].Clear();
    }

    public void BreakSection(TSection model)
    {
        CrossSectionGoBack.Take(model.Depth + 1).Flatten().Each(x => x.TargetSection = model);
        CrossSectionGoBack[model.Depth].Clear();
    }

    public void PageBreakSection(TSection model)
    {
        for (var i = 0; i < CrossSectionGoBack.Length; i++)
        {
            CrossSectionGoBack[i].Each(x => x.TargetSection ??= model);
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

    public void DataBindCancel(T data) => SummaryCancelAction.Each(x => x(data));

    public object GetSummary(ISummaryElement x, T data)
    {
        switch (x.SummaryType)
        {
            case SummaryTypes.Summary: return Mapper[x.SummaryBind](data);
            case SummaryTypes.Count: return (int)Mapper[x.SummaryCount](data);

            case SummaryTypes.Average:
                dynamic count = Mapper[x.SummaryCount](data);
                if (count == 0) return x.NaN;
                return ((dynamic)Mapper[x.SummaryBind](data)) / count;

            case SummaryTypes.Maximum or SummaryTypes.Minimum: return Mapper[x.SummaryBind](data);
            case SummaryTypes.PageCount: return SummaryPool["#:PAGECOUNT()"].Value!;
        }
        throw new();
    }

    public static IEnumerable<(string[] BreakKeys, ISummaryElement SummaryElement)> TraversSummaryElement(string[] keys, IParentSection section)
    {
        if (section is IBreakKey sec && sec.BreakKey != "") keys = [.. keys, sec.BreakKey];

        if (section.Header is ISection header) foreach (var x in TraversSummaryElement(keys, header.Elements)) yield return x;
        if (section.SubSection is ISection detail) foreach (var x in TraversSummaryElement(keys, detail.Elements)) yield return x;
        if (section.SubSection is IParentSection subsection) foreach (var x in TraversSummaryElement(keys, subsection)) yield return x;
        if (section.Footer is ISection footer) foreach (var x in TraversSummaryElement(keys, footer.Elements)) yield return x;
    }

    public static IEnumerable<(string[] BreakKeys, ISummaryElement SummaryElement)> TraversSummaryElement(string[] keys, IElement[] elements) => elements.OfType<ISummaryElement>().Select(x => (keys, x));
}

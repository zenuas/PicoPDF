using Extensions;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Mapper;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Binder;

public class BindSummaryMapper<T>
{
    public Dictionary<string, Func<T, object>> Mapper { get; init; } = ObjectMapper.CreateGetMapper<T>();
    public Dictionary<string, ClearableDynamicValue> SummaryPool { get; init; } = new();
    public List<Action<T>> SummaryAction { get; init; } = new();
    public List<List<(SummaryElement SummaryElement, TextModel TextModel)>> SummaryGoBack { get; init; } = new();

    public void CreatePool(PageSection page, string[] allkeys)
    {
        SummaryPool.Add("#:PAGECOUNT()", new ClearableDynamicValue() { Value = 1, Clear = _ => { } });
        TraversSummaryElement([], page).Each(sr =>
        {
            var keys = sr.SummaryElement.BreakKey == "" ? sr.BreakKeys : allkeys.Take(allkeys.FindLastIndex(x => x == sr.SummaryElement.BreakKey) + 1).ToArray();
            var breakpoint =
                sr.SummaryElement.SummaryMethod == SummaryMethod.All ? "#" :
                sr.SummaryElement.SummaryMethod == SummaryMethod.Page ? $"&{keys.Join(".")}" :
                $"${keys.Join(".")}";
            var bind = sr.SummaryElement.Bind;
            switch (sr.SummaryElement.SummaryType)
            {
                case SummaryType.Summary:
                case SummaryType.Average:
                    {
                        var key = $"{breakpoint}:{bind}";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = 0, Clear = x => x.Value = 0 };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value);
                            SummaryAction.Add(x => v.Value += (dynamic)Mapper[bind](x));
                        }
                        sr.SummaryElement.Bind = key;
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
                        sr.SummaryElement.SummaryBind = key;
                    }
                    break;

                case SummaryType.Maximum:
                    {
                        var key = $"{breakpoint}:MAX({bind})";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => x.Value = null };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value!);
                            SummaryAction.Add(x =>
                            {
                                var value = (dynamic)Mapper[bind](x);
                                v.Value = v.Value is null ? value : (v.Value < value ? value : v.Value);
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                    }
                    break;

                case SummaryType.Minimum:
                    {
                        var key = $"{breakpoint}:MIN({bind})";
                        if (!SummaryPool.ContainsKey(key))
                        {
                            var v = new ClearableDynamicValue() { Value = null, Clear = x => x.Value = null };
                            SummaryPool.Add(key, v);
                            Mapper.Add(key, _ => v.Value!);
                            SummaryAction.Add(x =>
                            {
                                var value = (dynamic)Mapper[bind](x);
                                v.Value = v.Value is null ? value : (v.Value > value ? value : v.Value);
                            });
                        }
                        sr.SummaryElement.SummaryBind = key;
                    }
                    break;
            }
        });
    }

    public void CreateSummaryGoBack(int hierarchy_count) => Lists.RangeTo(0, hierarchy_count + 1).Each(_ => SummaryGoBack.Add(new()));

    public void AddSummaryGoBack(SummaryElement summary, TextModel model, int hierarchy_count)
    {
        switch (summary.SummaryMethod)
        {
            case SummaryMethod.All:
                SummaryGoBack[0].Add((summary, model));
                break;

            case SummaryMethod.Page:
                SummaryGoBack[hierarchy_count + 2].Add((summary, model));
                break;

            case SummaryMethod.Group:
                SummaryGoBack[hierarchy_count + 2].Add((summary, model));
                break;
        }
    }

    public void PageBreak(T data)
    {
        SummaryGoBack[1].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format));
        SummaryGoBack[1].Clear();
        SummaryPool.Keys.Where(x => x.StartsWith("&")).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void KeyBreak(T data, int hierarchy_count, string[] allkeys)
    {
        for (int i = SummaryGoBack.Count - hierarchy_count; i < SummaryGoBack.Count; i++)
        {
            SummaryGoBack[i].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format));
            SummaryGoBack[i].Clear();
        }
    }

    public void LastBreak(T data)
    {
        SummaryGoBack[0].Each(x => x.TextModel.Text = BindFormat(GetSummary(x.SummaryElement, data), x.SummaryElement.Format));
        SummaryGoBack[0].Clear();
    }

    public static string BindFormat(object? o, string format) => (format == "" ? o?.ToString() : o?.Cast<IFormattable>()?.ToString(format, null)) ?? "";

    public void SetPageCount(int pagecount) => SummaryPool["#:PAGECOUNT()"].Value = pagecount;

    public void Clear(string[] nobreaks)
    {
        var nobreak = nobreaks.Join(".");
        var sumkey_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
        SummaryPool.Keys.Where(x => x.StartsWith(sumkey_prefix)).Each(x => SummaryPool[x].Clear(SummaryPool[x]));

        var pagekey_prefix = $"&{(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
        SummaryPool.Keys.Where(x => x.StartsWith(pagekey_prefix)).Each(x => SummaryPool[x].Clear(SummaryPool[x]));
    }

    public void DataBind(T data) => SummaryAction.Each(x => x(data));

    public object GetSummary(SummaryElement x, T data)
    {
        switch (x.SummaryType)
        {
            case SummaryType.Summary:
                return Mapper[x.Bind](data);

            case SummaryType.Count:
                return (int)Mapper[x.SummaryBind](data);

            case SummaryType.Average:
                return (dynamic)Mapper[x.Bind](data) / (dynamic)Mapper[x.SummaryBind](data);

            case SummaryType.Maximum:
            case SummaryType.Minimum:
                return Mapper[x.SummaryBind](data);

            case SummaryType.PageCount:
                return SummaryPool["#:PAGECOUNT()"].Value!;
        }
        throw new();
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

    public static IEnumerable<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, List<IElement> elements) => elements.OfType<SummaryElement>().Select(x => (keys, x));
}

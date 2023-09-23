using Extensions;
using PicoPDF.Mapper;
using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Section;

public class BindSummaryMapper<T>
{
    public Dictionary<string, Func<T, object>> Mapper { get; init; } = ObjectMapper.CreateGetMapper<T>();
    public Dictionary<string, dynamic> SummaryPool { get; init; } = new();
    public List<Action<T>> SummaryAction { get; init; } = new();

    public void CreatePool(PageSection page)
    {
        TraversSummaryElement([], page).Each(sr =>
        {
            var key = sr.BreakKeys.Join(".");
            var bind = sr.SummaryElement.Bind;
            var sumkey = $"${key}:{bind}";
            var countkey = $"${key}#";
            switch (sr.SummaryElement.SummaryType)
            {
                case SummaryType.Summary:
                    countkey = "";
                    break;

                case SummaryType.Count:
                    sumkey = "";
                    break;

                case SummaryType.Average:
                    break;
            }
            if (sumkey != "" && SummaryPool.TryAdd(sumkey, 0))
            {
                Mapper.Add(sumkey, _ => SummaryPool[sumkey]);
                SummaryAction.Add(x => SummaryPool[sumkey] += (dynamic)Mapper[bind](x));
            }
            if (countkey != "" && SummaryPool.TryAdd(countkey, 0))
            {
                Mapper.Add(countkey, _ => SummaryPool[countkey]);
                SummaryAction.Add(x => SummaryPool[countkey]++);
            }
            if (sumkey != "") sr.SummaryElement.Bind = sumkey;
            if (countkey != "") sr.SummaryElement.CountBind = countkey;
        });
    }

    public void Clear(string[] nobreaks)
    {
        var nobreak = nobreaks.Join(".");
        var sumkey_prefix = $"${(nobreak.Length > 0 ? $"{nobreak}." : nobreak)}";
        SummaryPool.Keys.Where(x => x.StartsWith(sumkey_prefix)).Each(x => SummaryPool[x] = 0);
    }

    public void DataBind(T data) => SummaryAction.Each(x => x(data));

    public object GetSummary(SummaryElement x, T data)
    {
        switch (x.SummaryType)
        {
            case SummaryType.Summary:
                return Mapper[x.Bind](data);

            case SummaryType.Count:
                return (int)Mapper[x.CountBind](data);

            case SummaryType.Average:
                return (dynamic)Mapper[x.Bind](data) / (dynamic)Mapper[x.CountBind](data);
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

    public static IEnumerable<(string[] BreakKeys, SummaryElement SummaryElement)> TraversSummaryElement(string[] keys, List<ISectionElement> elements) => elements.OfType<SummaryElement>().Select(x => (keys, x));
}

using Mina.Extension;
using Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Elements;

public class ElementDictionary : ElementValue, IHaveReferences
{
    public Dictionary<string, ElementValue> Dictionary { get; init; } = [];

    public override string ToElementString(IConverter? converter) => $"<< {Dictionary.Select(x => $"/{x.Key} {x.Value.ToElementString(converter)}").Join(" ")} >>";

    public IEnumerable<IHaveReferences> GetReferences()
    {
        foreach (var v in Dictionary.Values.OfType<IHaveReferences>())
        {
            yield return v;
        }
    }
}

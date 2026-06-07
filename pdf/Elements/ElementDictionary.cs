using Mina.Extension;
using Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Elements;

public class ElementDictionary : ElementValue
{
    public Dictionary<string, ElementValue> Dictionary { get; init; } = [];

    public override string ToElementString(IConverter? converter) => $"<< {Dictionary.Select(x => $"/{x.Key} {x.Value.ToElementString(converter)}").Join(" ")} >>";
}

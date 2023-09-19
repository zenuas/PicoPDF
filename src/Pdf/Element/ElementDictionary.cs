using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Element;

public class ElementDictionary : ElementValue
{
    public Dictionary<string, ElementValue> Dictionary { get; init; } = new();

    public override string ToElementString() => $"<< {Dictionary.Select(x => $"/{x.Key} {x.Value.ToElementString()}").Join(" ")} >>";
}

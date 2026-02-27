using Mina.Extension;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Elements;

public class ElementDictionary : ElementValue
{
    public Dictionary<string, ElementValue> Dictionary { get; init; } = [];

    public override string ToElementString() => $"<< {Dictionary.Select(x => $"/{x.Key} {x.Value.ToElementString()}").Join(" ")} >>";
}

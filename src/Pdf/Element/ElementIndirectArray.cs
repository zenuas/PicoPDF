using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Element;

public class ElementIndirectArray : ElementValue
{
    public List<ElementIndirectObject> Array { get; init; } = new();

    public ElementIndirectArray(params ElementIndirectObject[] xs)
    {
        Array.AddRange(xs);
    }

    public ElementIndirectArray(IEnumerable<ElementIndirectObject> xs)
    {
        Array.AddRange(xs);
    }

    public override string ToElementString() => $"[ {Array.Select(x => $"{x.ToElementString()}").Join(" ")} ]";
}

using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Element;

public class ElementIntegerArray : ElementValue
{
    public List<long> Array { get; init; } = new();

    public ElementIntegerArray(params long[] xs)
    {
        Array.AddRange(xs);
    }

    public ElementIntegerArray(IEnumerable<long> xs)
    {
        Array.AddRange(xs);
    }

    public override string ToElementString() => $"[ {Array.Select(x => $"{x}").Join(" ")} ]";
}

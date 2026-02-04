using Mina.Extension;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Element;

public class ElementArray<T> : ElementValue where T : ElementValue
{
    public List<T> Array { get; init; } = [];

    public ElementArray(params T[] xs) => Array.AddRange(xs);

    public ElementArray(IEnumerable<T> xs) => Array.AddRange(xs);

    public override string ToElementString() => $"[ {Array.Select(x => x.ToElementString()).Join(" ")} ]";
}

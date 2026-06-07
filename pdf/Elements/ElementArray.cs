using Mina.Extension;
using Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Elements;

public class ElementArray<T> : ElementValue where T : ElementValue
{
    public List<T> Array { get; init; } = [];

    public ElementArray(params T[] xs) => Array.AddRange(xs);

    public ElementArray(IEnumerable<T> xs) => Array.AddRange(xs);

    public override string ToElementString(IConverter? converter) => $"[ {Array.Select(x => x.ToElementString(converter)).Join(" ")} ]";
}

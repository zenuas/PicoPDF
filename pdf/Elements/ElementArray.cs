using Mina.Extension;
using Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Elements;

public class ElementArray<T> : ElementValue, IHaveReferences where T : ElementValue
{
    public IReadOnlyCollection<T> Array { get; init; } = [];

    public ElementArray(params T[] xs) => Array = xs;

    public ElementArray(IEnumerable<T> xs) => Array = [.. xs];

    public override string ToElementString(IConverter? converter) => $"[ {Array.Select(x => x.ToElementString(converter)).Join(" ")} ]";

    public IEnumerable<IHaveReferences> GetReferences()
    {
        foreach (var v in Array.OfType<IHaveReferences>())
        {
            yield return v;
        }
    }
}

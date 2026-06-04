using Mina.Extension;
using PicoPDF.Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Elements;

public class ElementArray<T> : ElementValue where T : ElementValue
{
    public List<T> Array { get; init; } = [];

    public ElementArray(params T[] xs) => Array.AddRange(xs);

    public ElementArray(IEnumerable<T> xs) => Array.AddRange(xs);

    public override string ToElementString(int object_number, int generation_number, ISecurityHandler? handler) => $"[ {Array.Select(x => x.ToElementString(object_number, generation_number, handler)).Join(" ")} ]";
}

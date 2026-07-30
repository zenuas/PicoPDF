using Pdf.Documents.Security;
using System.Collections.Generic;

namespace Pdf.Elements;

public class ElementIndirectObject : ElementValue, IHaveReferences
{
    public required IPdfObject References { get; init; }

    public override string ToElementString(IConverter? _) => $"{References.IndirectIndex} 0 R";

    public static implicit operator ElementIndirectObject(PdfObject x) => new() { References = x };

    public IEnumerable<IHaveReferences> GetReferences()
    {
        yield return References;
    }
}

using Pdf.Documents.Security;

namespace Pdf.Elements;

public class ElementIndirectObject : ElementValue
{
    public required IPdfObject References { get; init; }

    public override string ToElementString(IConverter? _) => $"{References.IndirectIndex} 0 R";

    public static implicit operator ElementIndirectObject(PdfObject x) => new() { References = x };
}

using Pdf.Documents.Security;

namespace Pdf.Elements;

public class ElementBoolean : ElementValue
{
    public required bool Value { get; set; }

    public override string ToElementString(IConverter? _) => Value ? "true" : "false";

    public static implicit operator ElementBoolean(bool x) => new() { Value = x };
}
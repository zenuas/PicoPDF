using Pdf.Documents.Security;

namespace Pdf.Elements;

public class ElementInteger : ElementValue
{
    public required long Value { get; init; }

    public override string ToElementString(IConverter? _) => $"{Value}";

    public static implicit operator ElementInteger(int x) => new() { Value = x };

    public static implicit operator ElementInteger(long x) => new() { Value = x };
}

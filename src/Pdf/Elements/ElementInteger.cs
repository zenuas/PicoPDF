using PicoPDF.Pdf.Documents.Security;

namespace PicoPDF.Pdf.Elements;

public class ElementInteger : ElementValue
{
    public required long Value { get; set; }

    public override string ToElementString(int object_number, int generation_number, ISecurityHandler? handler) => $"{Value}";

    public static implicit operator ElementInteger(int x) => new() { Value = x };

    public static implicit operator ElementInteger(long x) => new() { Value = x };
}

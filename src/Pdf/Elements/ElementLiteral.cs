using PicoPDF.Pdf.Documents.Security;

namespace PicoPDF.Pdf.Elements;

public class ElementLiteral : ElementValue
{
    public required string Value { get; set; }

    public override string ToElementString(int object_number, int generation_number, ISecurityHandler? handler) => Value;

    public static implicit operator ElementLiteral(string x) => new() { Value = x };
}
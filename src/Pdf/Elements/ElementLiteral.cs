namespace PicoPDF.Pdf.Elements;

public class ElementLiteral : ElementValue
{
    public required string Value { get; set; }

    public override string ToElementString() => Value;

    public static implicit operator ElementLiteral(string x) => new() { Value = x };
}
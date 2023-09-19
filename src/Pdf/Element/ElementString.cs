namespace PicoPDF.Pdf.Element;

public class ElementString : ElementValue
{
    public required string Value { get; set; }

    public override string ToElementString() => Value;

    public static implicit operator ElementString(string x) => new() { Value = x };
}

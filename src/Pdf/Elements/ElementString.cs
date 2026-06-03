using PicoPDF.Pdf.Extension;
using System.Text;

namespace PicoPDF.Pdf.Elements;

public class ElementString : ElementValue
{
    public required string Value { get; set; }
    public Encoding Encoding { get; init; } = Encoding.UTF8;

    public override string ToElementString() => Value.ToEscapeString(Encoding);

    public static implicit operator ElementString(string x) => new() { Value = x };
}

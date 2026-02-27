namespace PicoPDF.Pdf.Elements;

public class ElementIndirectObject : ElementValue
{
    public required IPdfObject References { get; init; }

    public override string ToElementString() => $"{References.IndirectIndex} 0 R";

    public static implicit operator ElementIndirectObject(PdfObject x) => new() { References = x };
}

namespace PicoPDF.Pdf.Element;

public class ElementBytes : ElementValue
{
    public required byte[] Bytes { get; set; }

    public override string ToElementString() => throw new();

    public static implicit operator ElementBytes(byte[] xs) => new() { Bytes = xs };
}

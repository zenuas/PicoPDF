using Mina.Text;
using PicoPDF.Pdf.Documents.Security;
using PicoPDF.Pdf.Extension;
using System.Text;

namespace PicoPDF.Pdf.Elements;

public class ElementString : ElementValue
{
    public required string Value { get; set; }
    // For text strings encoded in Unicode, the first two bytes must be 254 followed by 255.
    // These two bytes represent the Unicode byte order marker, U+FEFF, indicating that the string is encoded in the UTF-16BE (big-endian) encoding scheme specified in the Unicode standard.
    public Encoding Encoding { get; init; } = UTF16WithBOM.UTF16_BEWithBOM;

    public override string ToElementString(int object_number, int generation_number, ISecurityHandler? handler) => handler is { } ? Value.ToEncryptString(Encoding, object_number, generation_number, handler) : Value.ToEscapeString(Encoding);

    public static implicit operator ElementString(string x) => new() { Value = x };
}

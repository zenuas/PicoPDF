using System.Text;

namespace PicoPDF.Pdf.Encoding;

public class UTF16WithBOM : UnicodeEncoding
{
    public static readonly UTF16WithBOM UTF16_BEWithBOM = new(true);
    public static readonly UTF16WithBOM UTF16_LEWithBOM = new(false);

    public UTF16WithBOM(bool bigEndian)
        : base(bigEndian, true)
    {
    }

    public UTF16WithBOM(bool bigEndian, bool throwOnInvalidByte)
        : base(bigEndian, true, throwOnInvalidByte)
    {
    }

    public override byte[] GetBytes(string s) => [.. GetPreamble(), .. base.GetBytes(s)];
}

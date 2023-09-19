
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class StandardType1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required StandardType1Fonts Font { get; init; }
    public required Encoding TextEncoding { get; init; }
    public static byte[] EscapeBytes = Encoding.ASCII.GetBytes("()\\");
    public static byte EscapeCharByte = Encoding.ASCII.GetBytes("\\")[0];

    public override void DoExport()
    {
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", "/Subtype1");
        _ = Elements.TryAdd("BaseFont", $"/{Font.GetAttributeOrDefault<FontNameAttribute>()!.Name}");
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return TextEncoding
            .GetBytes(s)
            .Select<byte, byte[]>(x => x.In(EscapeBytes) ? [EscapeCharByte, x] : [x])
            .Flatten()
            .Prepend(Encoding.ASCII.GetBytes("(")[0])
            .Concat(Encoding.ASCII.GetBytes(") Tj"));
    }
}

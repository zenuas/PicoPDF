using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Document.Font;

public class Type1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required string BaseFont { get; init; }
    public required string Encoding { get; init; }
    public FontDescriptor FontDescriptor { get; init; } = new();
    public required Encoding TextEncoding { get; init; }
    public int FirstChar { get; set; } = 0;
    public int LastChar { get => FirstChar + Widths.Count - 1; }
    public List<long> Widths { get; init; } = new();
    public static byte[] EscapeBytes = System.Text.Encoding.ASCII.GetBytes("()\\");
    public static byte EscapeCharByte = System.Text.Encoding.ASCII.GetBytes("\\")[0];

    public Type1Font()
    {
        RelatedObjects.Add(FontDescriptor);
    }

    public override void DoExport()
    {
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", $"/Type1");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("FontDescriptor", FontDescriptor);
        _ = Elements.TryAdd("FirstChar", FirstChar);
        _ = Elements.TryAdd("LastChar", LastChar);
        _ = Elements.TryAdd("Widths", Widths);
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return (TextEncoding ?? System.Text.Encoding.ASCII)
            .GetBytes(s)
            .Select<byte, byte[]>(x => x.In(EscapeBytes) ? [EscapeCharByte, x] : [x])
            .Flatten()
            .Prepend(System.Text.Encoding.ASCII.GetBytes("(")[0])
            .Concat(System.Text.Encoding.ASCII.GetBytes(") Tj"));
    }
}

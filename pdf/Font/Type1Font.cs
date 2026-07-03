using Mina.Extension;
using Pdf.Extension;
using System.Linq;

namespace Pdf.Font;

public class Type1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required string BaseFont { get; init; }
    public required string Encoding { get; init; }
    public required FontDescriptor FontDescriptor { get; init; }
    public required int FirstChar { get; init; }
    public int LastChar => FirstChar + Widths.Length - 1;
    public long[] Widths { get; init; } = [];

    public override void BeforeExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDescriptor);
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", "/Type1");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("FontDescriptor", FontDescriptor);
        _ = Elements.TryAdd("FirstChar", FirstChar);
        _ = Elements.TryAdd("LastChar", LastChar);
        _ = Elements.TryAdd("Widths", Widths);
    }

    public string CreateTextShowingOperator(string s) => $"{s.ToEscapeString(System.Text.Encoding.ASCII)} Tj";

    public static Type1Font Create(string name, string basefont, Type1Encodings encoding, FontDescriptorFlags flags = FontDescriptorFlags.Nonsymbolic)
    {
        var fontdict = new FontDescriptor()
        {
            FontName = basefont,
            Flags = flags,
            MissingWidth = 500,
        };
        return new()
        {
            Name = name,
            BaseFont = basefont,
            Encoding = encoding.ToString(),
            FontDescriptor = fontdict,
            FirstChar = 32,
            Widths = [.. Lists.Repeat(500L).Take(126 - 32 + 1)],
        };
    }
}

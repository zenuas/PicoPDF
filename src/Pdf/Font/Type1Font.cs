using Mina.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class Type1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required string BaseFont { get; init; }
    public required string Encoding { get; init; }
    public required FontDescriptor FontDescriptor { get; init; }
    public required Encoding TextEncoding { get; init; }
    public required int FirstChar { get; init; }
    public int LastChar { get => FirstChar + Widths.Count - 1; }
    public List<long> Widths { get; init; } = [];

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDescriptor);
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", $"/Type1");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("FontDescriptor", FontDescriptor);
        _ = Elements.TryAdd("FirstChar", FirstChar);
        _ = Elements.TryAdd("LastChar", LastChar);
        _ = Elements.TryAdd("Widths", Widths);
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s) => PdfUtility.ToStringEscapeBytes(s, TextEncoding).Concat(System.Text.Encoding.ASCII.GetBytes(" Tj"));
}

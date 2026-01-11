
using Mina.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class StandardType1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required StandardType1Fonts Font { get; init; }
    public required Encoding TextEncoding { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", "/Subtype1");
        _ = Elements.TryAdd("BaseFont", $"/{Font.GetAttributeOrDefault<FontNameAttribute>()!.Name}");
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s) => PdfUtility.ToStringEscapeBytes(s, TextEncoding).Concat(System.Text.Encoding.ASCII.GetBytes(" Tj"));
}

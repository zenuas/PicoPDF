
using Mina.Extension;
using Pdf.Extension;
using System.Text;

namespace Pdf.Font;

public class StandardType1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required StandardType1Fonts Font { get; init; }

    public override void BeforeExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", "/Subtype1");
        _ = Elements.TryAdd("BaseFont", $"/{Font.GetAttributeOrDefault<FontNameAttribute>()!.Name}");
    }

    public string CreateTextShowingOperator(string s) => $"{s.ToEscapeString(Encoding.ASCII)} Tj";
}


using Mina.Extension;

namespace PicoPDF.Pdf.Font;

public class StandardType1Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required StandardType1Fonts Font { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", "/Subtype1");
        _ = Elements.TryAdd("BaseFont", $"/{Font.GetAttributeOrDefault<FontNameAttribute>()!.Name}");
    }

    public string CreateTextShowingOperator(string s) => $"{PdfUtility.ToEscapeString(s)} Tj";
}

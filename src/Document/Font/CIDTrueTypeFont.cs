using Extensions;
using PicoPDF.Document.Element;
using PicoPDF.Document.Font.TrueType;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Document.Font;

public class CIDTrueTypeFont : PdfObject, IFont
{
    public required string Name { get; init; }
    public required TrueTypeFont Font { get; init; }
    public required string Encoding { get; init; }
    public CIDFontDictionary FontDictionary { get; init; } = new();

    public override void DoExport()
    {
        RelatedObjects.Add(FontDictionary);
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", $"/Type0");
        _ = Elements.TryAdd("BaseFont", $"/{Font.PostScriptName}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("DescendantFonts", new ElementIndirectArray(FontDictionary));
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return s
            .Select(x => System.Text.Encoding.ASCII.GetBytes($"{Font.CharToGID(x):x4}"))
            .Flatten()
            .Prepend(System.Text.Encoding.ASCII.GetBytes("<")[0])
            .Concat(System.Text.Encoding.ASCII.GetBytes("> Tj"));
    }
}

using Mina.Extensions;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Element;
using PicoPDF.TrueType;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class TrueTypeFont : PdfObject, IFont
{
    public required string Name { get; init; }
    public required TrueTypeFontInfo Font { get; init; }
    public required string Encoding { get; init; }
    public CIDFontDictionary FontDictionary { get; init; } = new();

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDictionary);
        _ = Elements.TryAdd("Type", $"/Font");
        _ = Elements.TryAdd("Subtype", $"/Type0");
        _ = Elements.TryAdd("BaseFont", $"/{Font.PostScriptName}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("DescendantFonts", new ElementIndirectArray(FontDictionary));
    }

    public Position MeasureStringBox(string s) => new()
    {
        Left = 0,
        Top = (double)(-Font.OS2.STypoAscender) / Font.FontHeader.UnitsPerEm,
        Right = (double)Font.MeasureString(s) / 1000,
        Bottom = (double)(-Font.OS2.STypoDescender) / Font.FontHeader.UnitsPerEm,
    };

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return s
            .Select(x => System.Text.Encoding.ASCII.GetBytes($"{Font.CharToGID(x):x4}"))
            .Flatten()
            .Prepend(System.Text.Encoding.ASCII.GetBytes("<")[0])
            .Concat(System.Text.Encoding.ASCII.GetBytes("> Tj"));
    }
}

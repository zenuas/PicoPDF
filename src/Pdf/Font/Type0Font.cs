using Mina.Extension;
using PicoPDF.OpenType;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Element;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class Type0Font : PdfObject, IFont
{
    public required string Name { get; init; }
    public required IOpenTypeRequiredTables Font { get; init; }
    public required string Encoding { get; init; }
    public CIDFontDictionary FontDictionary { get; init; } = new();
    public HashSet<char> Chars { get; init; } = [];

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDictionary);
        _ = Elements.TryAdd("Type", $"/Font");
        _ = Elements.TryAdd("Subtype", $"/Type0");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("DescendantFonts", new ElementIndirectArray(FontDictionary));
        if (option.AppendCIDToUnicode)
        {
            var cmap = new CIDToUnicode { Font = Font, Chars = Chars };
            RelatedObjects.Add(cmap);
            _ = Elements.TryAdd("ToUnicode", cmap);
        }
        if (option.EmbeddedFont && FontDictionary.FontDescriptor is { } descriptor)
        {
            _ = Elements.TryAdd("BaseFont", $"/ABCDEF+{Font.PostScriptName}");
            var fontfile = new PdfObject();
            RelatedObjects.Add(fontfile);
            var stream = fontfile.GetWriteStream(true);
            var fontdata = File.ReadAllBytes(Font.Path.Path);
            stream.Write(fontdata);
            if (Font.Offset.ContainTrueType())
            {
                _ = descriptor.Elements.TryAdd("FontFile2", fontfile);
                _ = fontfile.Elements.TryAdd("Length1", fontdata.Length);
            }
            else
            {
                _ = descriptor.Elements.TryAdd("FontFile3", fontfile);
                _ = fontfile.Elements.TryAdd("Subtype", "/OpenType");
            }
        }
        else
        {
            _ = Elements.TryAdd("BaseFont", $"/{Font.PostScriptName}");
        }
    }

    public Position MeasureStringBox(string s)
    {
        s.Each(x => Chars.Add(x));
        return new()
        {
            Left = 0,
            Top = (double)-Font.OS2.STypoAscender / Font.FontHeader.UnitsPerEm,
            Right = (double)Font.MeasureString(s) / 1000,
            Bottom = (double)-Font.OS2.STypoDescender / Font.FontHeader.UnitsPerEm,
        };
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return s
            .Select(x => System.Text.Encoding.ASCII.GetBytes($"{Font.CharToGIDCached(x):x4}"))
            .Flatten()
            .Prepend(System.Text.Encoding.ASCII.GetBytes("<")[0])
            .Concat(System.Text.Encoding.ASCII.GetBytes("> Tj"));
    }
}

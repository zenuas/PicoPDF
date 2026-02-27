using Mina.Extension;
using PicoPDF.OpenType;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Elements;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class Type0Font : PdfObject, IFont, IFontChars
{
    public required string Name { get; init; }
    public required IOpenTypeRequiredTables Font { get; init; }
    public IOpenTypeRequiredTables? EmbeddedFont { get; set; }
    public required IFontRegister FontRegister { get; init; }
    public required string Encoding { get; init; }
    public required CIDFontDictionary FontDictionary { get; init; }
    public HashSet<int> Chars { get; init; } = [];

    public void CreateEmbeddedFont()
    {
        var fontdata = FontRegister.LoadComplete(Font);
        EmbeddedFont = Font.Offset.ContainTrueType()
            ? FontExtract.Extract(fontdata.Cast<TrueTypeFont>(), new() { ExtractChars = [.. Chars] })
            : FontExtract.Extract(fontdata.Cast<PostScriptFont>(), new() { ExtractChars = [.. Chars] });
    }

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDictionary);
        _ = Elements.TryAdd("Type", "/Font");
        _ = Elements.TryAdd("Subtype", "/Type0");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("DescendantFonts", new ElementArray<ElementIndirectObject>(FontDictionary));
        if (option.AppendCIDToUnicode)
        {
            var cmap = new CIDToUnicode { Font = EmbeddedFont ?? Font, Chars = Chars };
            RelatedObjects.Add(cmap);
            _ = Elements.TryAdd("ToUnicode", cmap);
        }
        if (EmbeddedFont is { } && FontDictionary.FontDescriptor is { } descriptor)
        {
            _ = Elements.TryAdd("BaseFont", $"/ABCDEF+{EmbeddedFont.PostScriptName}");
            var fontfile = new PdfObject();
            RelatedObjects.Add(fontfile);
            var writer = fontfile.GetWriteStream(option.FontStreamDeflate);
            if (EmbeddedFont.Offset.ContainTrueType())
            {
                var export = FontExporter.Export(EmbeddedFont.Cast<TrueTypeFont>());
                _ = descriptor.Elements.TryAdd("FontFile2", fontfile);
                _ = fontfile.Elements.TryAdd("Length1", export.Length);
                writer.Write(export);
            }
            else
            {
                var export = FontExporter.Export(EmbeddedFont.Cast<PostScriptFont>());
                _ = descriptor.Elements.TryAdd("FontFile3", fontfile);
                _ = fontfile.Elements.TryAdd("Subtype", "/OpenType");
                writer.Write(export);
            }
            writer.Flush();
        }
        else
        {
            _ = Elements.TryAdd("BaseFont", $"/{Font.PostScriptName}");
        }

        var font = EmbeddedFont ?? Font;
        FontDictionary.W = new(Chars
                .Order()
                .Select(x => (Char: x, GID: font.CharToGID(x), Width: font.MeasureGID(font.CharToGID(x))))
                .Where(x => x.GID != 0 && (FontDictionary.DW is not { } dw || x.Width != dw))
                .Select(x => new ElementString { Value = $"{x.GID}[{x.Width}]{(option.Debug ? $" %{char.ConvertFromUtf32(x.Char)}" : "")}\n" })
            );
    }

    public FontBox MeasureStringBox(string s) => new()
    {
        Ascender = (double)-(Font.OS2?.STypoAscender ?? Font.HorizontalHeader.Ascender) / Font.FontHeader.UnitsPerEm,
        Descender = (double)-(Font.OS2?.STypoDescender ?? Font.HorizontalHeader.Descender) / Font.FontHeader.UnitsPerEm,
        LineGap = (double)Font.HorizontalHeader.LineGap / Font.FontHeader.UnitsPerEm,
        Width = (double)Font.MeasureString(s) / 1000,
    };

    public string CreateTextShowingOperator(string s) => $"<{s.ToUtf32CharArray().Select(x => $"{(EmbeddedFont ?? Font).CharToGID(x):x4}").Join()}> Tj";
}

using Mina.Extension;
using OpenType;
using Pdf.Documents;
using Pdf.Elements;
using Pdf.Extension;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Font;

public class Type0Font : PdfObject, IFont, IFontChars
{
    public required string Name { get; init; }
    public required IOpenTypeFont Font { get; init; }
    public required FontEmbeds FontEmbed { get; init; }
    public IOpenTypeFont? EmbeddedFont { get; set; }
    public required string Encoding { get; init; }
    public required CIDFontDictionary FontDictionary { get; init; }
    public HashSet<int> Chars { get; init; } = [];

    public void CreateEmbeddedFont() => EmbeddedFont = FontExtract.Extract(Font, new() { ExtractChars = [.. Chars] });

    public override void BeforeExport(PdfExportOption option)
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
            if ((FontEmbed & FontEmbeds.ConvertMask) == FontEmbeds.ConvertToTrueType ||
                ((FontEmbed & FontEmbeds.ConvertMask) == FontEmbeds.ConvertNone && Font.Offset.ContainTrueType()))
            {
                var export = FontExporter.Export(EmbeddedFont, FontTypes.TrueType);
                _ = descriptor.Elements.TryAdd("FontFile2", fontfile);
                _ = fontfile.Elements.TryAdd("Length1", export.Length);
                writer.Write(export);
            }
            else
            {
                var export = FontExporter.Export(EmbeddedFont, FontTypes.PostScript);
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
                .Select(x => (Char: x, GID: font.CharToGID(x), Width: font.MeasureGID(font.CharToGID(x)) * 1000))
                .Where(x => x.GID != 0 && (FontDictionary.DW is not { } dw || x.Width != dw))
                .Select(x => new ElementLiteral { Value = $"{x.GID}[{x.Width.ToPointString(option.PointFormat)}]{(option.Debug ? $" %{char.ConvertFromUtf32(x.Char)}" : "")}\n" })
            );
    }

    public string CreateTextShowingOperator(string s) => $"<{s.ToUtf32CharArray().Select(x => $"{(EmbeddedFont ?? Font).CharToGID(x):x4}").Join()}> Tj";

    public static Type0Font Create(string name, IOpenTypeFont font, FontEmbeds embed = FontEmbeds.PossibleEmbed)
    {
        var flag =
            (font.CMap.EncodingRecords.Contains(x => x.Key.PlatformID == (ushort)Platforms.Windows && x.Key.EncodingID == (ushort)Encodings.Windows_Symbol) ?
                FontDescriptorFlags.Symbolic :
                FontDescriptorFlags.Nonsymbolic) |
            (font.PostScript.IsFixedPitch != 0 ? FontDescriptorFlags.FixedPitch : 0) |
            ((font.FontHeader.MacStyle & 1) != 0 ? FontDescriptorFlags.ForceBold : 0) |
            ((font.FontHeader.MacStyle & 2) != 0 ? FontDescriptorFlags.Italic : 0);

        return Create(name, font, flag, embed);
    }

    public static Type0Font Create(string name, IOpenTypeFont font, FontDescriptorFlags flags, FontEmbeds embed)
    {
        var cmap = CMaps.Identity_H;
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var fontdict = new CIDFontDictionary()
        {
            Subtype = font.Offset.ContainTrueType() ? "CIDFontType2" : "CIDFontType0",
            BaseFont = font.PostScriptName,
            CIDSystemInfo = new()
            {
                Dictionary =
                {
                    ["Registry"] = new ElementString { Value = cidsysinfo.Registry },
                    ["Ordering"] = new ElementString { Value = cidsysinfo.Ordering },
                    ["Supplement"] = cidsysinfo.Supplement,
                }
            },
            DW = font.FontHeader.UnitsPerEm,
            FontDescriptor = new() { FontName = font.PostScriptName, Flags = flags },
        };
        return new()
        {
            Name = name,
            Font = font,
            FontEmbed = embed,
            Encoding = cidsysinfo.Name,
            FontDictionary = fontdict,
        };
    }
}

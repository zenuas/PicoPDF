using Mina.Extension;
using OpenType;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Font;
using System;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Documents;

public partial class Document
{
    public Func<string, FontEmbeds, Type0Font> CreateFontCache()
    {
        var fontcache = PdfObjects.OfType<Type0Font>().ToDictionary(x => x.Name, x => x);
        return (name, embed) =>
        {
            var namekey = $"{name};{embed}";
            if (fontcache.TryGetValue(namekey, out var value)) return value;
            var x = AddFont($"F{fontcache.Count}", FontRegister.LoadComplete(name), embed);
            fontcache.Add(namekey, x);
            return x;
        };
    }

    public CIDFont AddFont(string name, string basefont, CMaps cmap, Encoding enc, FontDescriptorFlags flags = FontDescriptorFlags.Nonsymbolic)
    {
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var fontdict = new CIDFontDictionary()
        {
            Subtype = "CIDFontType0",
            BaseFont = basefont,
            CIDSystemInfo = new()
            {
                Dictionary =
                {
                    ["Registry"] = new ElementString { Value = cidsysinfo.Registry },
                    ["Ordering"] = new ElementString { Value = cidsysinfo.Ordering },
                    ["Supplement"] = cidsysinfo.Supplement,
                }
            },
            FontDescriptor = new() { FontName = basefont, Flags = flags },
        };
        var font = new CIDFont()
        {
            Name = name,
            BaseFont = $"{basefont}-{cidsysinfo.Name}",
            Encoding = cidsysinfo.Name,
            TextEncoding = enc,
            FontDictionary = fontdict,
        };
        PdfObjects.Add(font);

        return font;
    }

    public Type0Font AddFont(string name, IOpenTypeFont font, FontEmbeds embed = FontEmbeds.PossibleEmbed)
    {
        var flag =
            (font.CMap.EncodingRecords.Contains(x => x.Key.PlatformID == (ushort)Platforms.Windows && x.Key.EncodingID == (ushort)Encodings.Windows_Symbol) ?
                FontDescriptorFlags.Symbolic :
                FontDescriptorFlags.Nonsymbolic) |
            (font.PostScript.IsFixedPitch != 0 ? FontDescriptorFlags.FixedPitch : 0) |
            ((font.FontHeader.MacStyle & 1) != 0 ? FontDescriptorFlags.ForceBold : 0) |
            ((font.FontHeader.MacStyle & 2) != 0 ? FontDescriptorFlags.Italic : 0);

        return AddFont(name, font, flag, embed);
    }

    public Type0Font AddFont(string name, IOpenTypeFont font, FontDescriptorFlags flags, FontEmbeds embed)
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
        var type0 = new Type0Font()
        {
            Name = name,
            Font = font,
            FontEmbed = embed,
            FontRegister = FontRegister,
            Encoding = cidsysinfo.Name,
            FontDictionary = fontdict,
        };
        PdfObjects.Add(type0);

        return type0;
    }

    public Type1Font AddFont(string name, string basefont, Type1Encodings encoding, FontDescriptorFlags flags = FontDescriptorFlags.Nonsymbolic)
    {
        var fontdict = new FontDescriptor()
        {
            FontName = basefont,
            Flags = flags,
            MissingWidth = 500,
        };
        var font = new Type1Font()
        {
            Name = name,
            BaseFont = basefont,
            Encoding = encoding.ToString(),
            FontDescriptor = fontdict,
            FirstChar = 32,
            Widths = [.. Lists.Repeat(500L).Take(126 - 32 + 1)],
        };
        PdfObjects.Add(font);

        return font;
    }

    public StandardType1Font AddFont(string name, StandardType1Fonts fontname)
    {
        var font = new StandardType1Font() { Name = name, Font = fontname };
        PdfObjects.Add(font);

        return font;
    }
}

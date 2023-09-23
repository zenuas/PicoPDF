using Extensions;
using PicoPDF.Pdf.Element;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using PicoPDF.TrueType;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf;

public class Document
{
    public int Version { get; init; } = 17;
    public List<PdfObject> PdfObjects { get; init; } = new();
    public PdfObject Catalog { get; init; } = new()
    {
        Elements = new()
        {
            { "Type", "/Catalog" },
        }
    };
    public PdfObject PageTree { get; init; } = new()
    {
        Elements = new()
        {
            { "Type", "/Pages" },
            { "Count", 0 },
            { "Kids", new ElementIndirectArray() },
        }
    };
    public FontRegister FontRegister { get; init; } = new();

    public Document()
    {
        _ = Catalog.Elements.TryAdd("Pages", PageTree);

        PdfObjects.Add(Catalog);
        PdfObjects.Add(PageTree);
    }

    public Page NewPage(PageSize size, Orientation orientation = Orientation.Vertical)
    {
        var page = new Page() { Document = this, Size = size, Orientation = orientation };
        page.Elements["Parent"] = PageTree;
        PdfObjects.Add(page);

        PageTree.Elements["Count"].Cast<ElementInteger>().Value += 1;
        PageTree.Elements["Kids"].Cast<ElementIndirectArray>().Array.Add(page);
        return page;
    }

    public CIDFont AddFont(string name, string basefont, CMap cmap, Encoding enc, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
    {
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var font = new CIDFont() { Name = name, BaseFont = $"{basefont}-{cidsysinfo.Name}", Encoding = cidsysinfo.Name, TextEncoding = enc };
        PdfObjects.Add(font);

        font.FontDictionary.BaseFont = basefont;
        font.FontDictionary.CIDSystemInfo.Dictionary["Registry"] = $"({cidsysinfo.Registry})";
        font.FontDictionary.CIDSystemInfo.Dictionary["Ordering"] = $"({cidsysinfo.Ordering})";
        font.FontDictionary.CIDSystemInfo.Dictionary["Supplement"] = cidsysinfo.Supplement;
        font.FontDictionary.FontDescriptor = new FontDescriptor() { FontName = basefont, Flags = flags };

        return font;
    }

    public TrueTypeFont AddFont(string name, TrueTypeFontInfo ttf, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
    {
        var cmap = CMap.Identity_H;
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var fontdict = new CIDFontDictionary() { Subtype = "CIDFontType2 ", BaseFont = ttf.PostScriptName };
        var font = new TrueTypeFont() { Name = name, Font = ttf, Encoding = cidsysinfo.Name, FontDictionary = fontdict };
        PdfObjects.Add(font);

        font.FontDictionary.W = new ElementStringArray(
                Lists.RangeTo(' ', '~')
                    .Select(x => (Char: x, GID: ttf.CharToGID(x)))
                    .Select(x => $"{x.GID}[{ttf.MeasureGID(x.GID)}] %{x.Char}\n")
            );
        font.FontDictionary.CIDSystemInfo.Dictionary["Registry"] = $"({cidsysinfo.Registry})";
        font.FontDictionary.CIDSystemInfo.Dictionary["Ordering"] = $"({cidsysinfo.Ordering})";
        font.FontDictionary.CIDSystemInfo.Dictionary["Supplement"] = cidsysinfo.Supplement;
        font.FontDictionary.FontDescriptor = new FontDescriptor() { FontName = ttf.PostScriptName, Flags = flags };

        return font;
    }

    public Type1Font AddFont(string name, string basefont, Type1Encoding encoding, Encoding enc, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
    {
        var font = new Type1Font() { Name = name, BaseFont = basefont, Encoding = encoding.ToString(), TextEncoding = enc };
        PdfObjects.Add(font);

        font.FontDescriptor.FontName = basefont;
        font.FontDescriptor.Flags = flags;
        font.FontDescriptor.MissingWidth = 500;

        font.FirstChar = 32;
        font.Widths.AddRange(Lists.Repeat(500L).Take(126 - 32 + 1));

        return font;
    }

    public StandardType1Font AddFont(string name, StandardType1Fonts fontname, Encoding enc)
    {
        var font = new StandardType1Font() { Name = name, Font = fontname, TextEncoding = enc };
        PdfObjects.Add(font);

        return font;
    }

    public ImageXObject AddImage(string name, string path)
    {
        var image = new ImageXObject() { Name = name, Path = path };
        PdfObjects.Add(image);

        return image;
    }

    public void Save(string path, PdfExportOption? option = null)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        PdfExport.Export(this, stream, option ?? new());
    }
}

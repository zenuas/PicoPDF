using Mina.Extension;
using PicoPDF.Image;
using PicoPDF.Image.Png;
using PicoPDF.OpenType;
using PicoPDF.Pdf.Element;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf;

public class Document
{
    public int Version { get; init; } = 17;
    public List<PdfObject> PdfObjects { get; init; } = [];
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
        font.FontDictionary.FontDescriptor = new() { FontName = basefont, Flags = flags };

        return font;
    }

    public Type0Font AddFont(string name, FontInfo font, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
    {
        var cmap = CMap.Identity_H;
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var fontdict = new CIDFontDictionary() { Subtype = font.Offset.ContainTrueType() ? "CIDFontType2" : "CIDFontType0", BaseFont = font.PostScriptName };
        var type0 = new Type0Font() { Name = name, Font = font, Encoding = cidsysinfo.Name, FontDictionary = fontdict };
        PdfObjects.Add(type0);

        type0.FontDictionary.W = new(
                Lists.RangeTo(' ', '~')
                    .Select(x => (Char: x, GID: font.CharToGIDCached(x)))
                    .Select(x => $"{x.GID}[{font.MeasureGID(x.GID)}] %{x.Char}\n")
            );
        type0.FontDictionary.CIDSystemInfo.Dictionary["Registry"] = $"({cidsysinfo.Registry})";
        type0.FontDictionary.CIDSystemInfo.Dictionary["Ordering"] = $"({cidsysinfo.Ordering})";
        type0.FontDictionary.CIDSystemInfo.Dictionary["Supplement"] = cidsysinfo.Supplement;
        type0.FontDictionary.FontDescriptor = new() { FontName = font.PostScriptName, Flags = flags };

        return type0;
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

    public IImageXObject AddImage(string name, string path, int width, int height)
    {
        IImageXObject image = ImageLoader.TypeCheck(path) switch
        {
            ImageTypes.Jpeg => new JpegXObject() { Name = name, Width = width, Height = height, Path = path },
            ImageTypes.Png => new ImageXObject() { Name = name, Width = width, Height = height, Canvas = ImageLoader.FromFile(path, ImageTypes.Png)!.Cast<PngFile>().Canvas },
            _ => throw new(),
        };
        PdfObjects.Add(image.Cast<PdfObject>());

        return image;
    }

    public void Save(string path, PdfExportOption? option = null)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        Save(stream, option ?? new());
    }

    public void Save(Stream stream, PdfExportOption? option = null) => PdfExport.Export(this, stream, option ?? new());
}

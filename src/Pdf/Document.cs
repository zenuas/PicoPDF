using Image;
using Image.Png;
using Mina.Extension;
using Mina.Text;
using OpenType;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System;
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
            { "Kids", new ElementArray<ElementIndirectObject>() },
        }
    };
    public PdfObject? Info { get; set; }
    public required IFontRegister FontRegister { get; init; }

    public Document()
    {
        _ = Catalog.Elements.TryAdd("Pages", PageTree);

        PdfObjects.Add(Catalog);
        PdfObjects.Add(PageTree);
    }

    public Page NewPage(int width, int height)
    {
        var page = new Page() { Document = this, Width = width, Height = height };
        page.Elements["Parent"] = PageTree;
        PdfObjects.Add(page);

        PageTree.Elements["Count"].Cast<ElementInteger>().Value += 1;
        PageTree.Elements["Kids"].Cast<ElementArray<ElementIndirectObject>>().Array.Add(page);
        return page;
    }

    public CIDFont AddFont(string name, string basefont, CMap cmap, Encoding enc, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
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
                    ["Registry"] = $"({cidsysinfo.Registry})",
                    ["Ordering"] = $"({cidsysinfo.Ordering})",
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

    public Type0Font AddFont(string name, IOpenTypeRequiredTables font)
    {
        var flag =
            (font.CMap.EncodingRecords.Contains(x => x.Key.PlatformEncoding == PlatformEncodings.Windows_Symbol) ?
                FontDescriptorFlags.Symbolic :
                FontDescriptorFlags.Nonsymbolic) |
            (font.PostScript.IsFixedPitch != 0 ? FontDescriptorFlags.FixedPitch : 0) |
            ((font.FontHeader.MacStyle & 1) != 0 ? FontDescriptorFlags.ForceBold : 0) |
            ((font.FontHeader.MacStyle & 2) != 0 ? FontDescriptorFlags.Italic : 0);

        return AddFont(name, font, flag);
    }

    public Type0Font AddFont(string name, IOpenTypeRequiredTables font, FontDescriptorFlags flags)
    {
        var cmap = CMap.Identity_H;
        var cidsysinfo = cmap.GetAttributeOrDefault<CIDSystemInfoAttribute>()!;
        var fontdict = new CIDFontDictionary()
        {
            Subtype = font.Offset.ContainTrueType() ? "CIDFontType2" : "CIDFontType0",
            BaseFont = font.PostScriptName,
            CIDSystemInfo = new()
            {
                Dictionary =
                {
                    ["Registry"] = $"({cidsysinfo.Registry})",
                    ["Ordering"] = $"({cidsysinfo.Ordering})",
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
            FontRegister = FontRegister,
            Encoding = cidsysinfo.Name,
            FontDictionary = fontdict,
        };
        PdfObjects.Add(type0);

        return type0;
    }

    public Type1Font AddFont(string name, string basefont, Type1Encoding encoding, FontDescriptorFlags flags = FontDescriptorFlags.Serif)
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

    public PdfObject AddInfo(
            string? title = null,
            string? author = null,
            string? subject = null,
            string? keywords = null,
            string? creator = null,
            string? producer = null,
            DateTime? creation_date = null,
            DateTime? mod_date = null,
            string? trapped = null
        )
    {
        if (Info is { }) PdfObjects.Remove(Info);
        PdfObjects.Add(Info = new());
        if (title is { }) Info.Elements.Add("Title", PdfUtility.ToEscapeString(title, UTF16WithBOM.UTF16_BEWithBOM));
        if (author is { }) Info.Elements.Add("Author", PdfUtility.ToEscapeString(author, UTF16WithBOM.UTF16_BEWithBOM));
        if (subject is { }) Info.Elements.Add("Subject", PdfUtility.ToEscapeString(subject, UTF16WithBOM.UTF16_BEWithBOM));
        if (keywords is { }) Info.Elements.Add("Keywords", PdfUtility.ToEscapeString(keywords, UTF16WithBOM.UTF16_BEWithBOM));
        if (creator is { }) Info.Elements.Add("Creator", PdfUtility.ToEscapeString(creator, UTF16WithBOM.UTF16_BEWithBOM));
        if (producer is { }) Info.Elements.Add("Producer", PdfUtility.ToEscapeString(producer, UTF16WithBOM.UTF16_BEWithBOM));
        if (creation_date is { }) Info.Elements.Add("CreationDate", creation_date);
        if (mod_date is { }) Info.Elements.Add("ModDate", mod_date);
        if (trapped is { }) Info.Elements.Add("Trapped", trapped);
        return Info;
    }

    public void Save(string path, PdfExportOption? option = null)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        Save(stream, option ?? new());
    }

    public void Save(Stream stream, PdfExportOption? option = null) => PdfExport.Export(this, stream, option ?? new());
}

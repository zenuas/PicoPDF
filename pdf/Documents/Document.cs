using Mina.Extension;
using Pdf.Elements;
using Pdf.Font;
using Pdf.XObject.Image;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public int Version { get; init; } = 17;
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
    public required IFontRegister FontRegister { get; init; }
    public List<Page> Pages { get; init; } = [];
    public Func<string, FontEmbeds, Type0Font> GetFont { get; init; }
    public Func<string, IImageXObject> GetImage { get; init; }

    public Document()
    {
        _ = Catalog.Elements.TryAdd("Pages", PageTree);

        GetFont = CreateFontCache();
        GetImage = CreateImageCache();
    }

    public Page NewPage(int width, int height)
    {
        var page = new Page() { Document = this, Width = width, Height = height };
        page.Elements["Parent"] = PageTree;
        Pages.Add(page);

        PageTree.Elements["Count"].Cast<ElementInteger>().Value += 1;
        PageTree.Elements["Kids"].Cast<ElementArray<ElementIndirectObject>>().Array.Add(page);
        return page;
    }

    public void Save(string path, PdfExportOption? option = null)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        Save(stream, option ?? new());
    }

    public void Save(Stream stream, PdfExportOption? option = null) => PdfExport.Export(this, stream, option ?? new());

    public IEnumerable<PdfObject> GetPdfObjects()
    {
        foreach (var x in Pages) yield return x;
        foreach (var x in Fonts.OfType<PdfObject>()) yield return x;
        foreach (var x in Images.OfType<PdfObject>()) yield return x;
        foreach (var x in Shadings.OfType<PdfObject>()) yield return x;
        foreach (var x in GraphicsStateParameters.OfType<PdfObject>()) yield return x;

        yield return PageTree;
        yield return Catalog;

        if (Info is { }) yield return Info;
        if (Encrypt is PdfObject encrypt) yield return encrypt;
    }
}

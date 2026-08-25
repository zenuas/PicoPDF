using Pdf.Documents.Security;
using Pdf.Elements;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Pdf.Documents;

public class Document : IHaveReferences
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
        }
    };
    public List<Page> Pages { get; init; } = [];
    public required Resources Resources { get; init; }
    public TrailerInfo? Info { get; init; } = null;
    public IStandardEncryption? Encrypt { get; init; } = null;
    public (byte[] CreateID, byte[] UpdateID)? DocumentID { get; init; }

    public Document()
    {
        _ = Catalog.Elements.TryAdd("Pages", PageTree);
    }

    public Page NewPage(int width, int height)
    {
        var page = new Page() { Width = width, Height = height };
        page.Elements["Parent"] = PageTree;
        Pages.Add(page);

        return page;
    }

    public void Save(string path, PdfExportOption? option = null)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        Save(stream, option ?? new());
    }

    public void Save(Stream stream, PdfExportOption? option = null) => PdfExport.Export(this, stream, option ?? new());

    public virtual IEnumerable<IHaveReferences> GetReferences()
    {
        foreach (var r in Resources.GetReferences())
        {
            yield return r;
        }

        PageTree.Elements["Count"] = Pages.Count;
        PageTree.Elements["Kids"] = Pages.Select(x => new ElementIndirectObject() { References = x }).ToArray();
        yield return PageTree;
        yield return Catalog;

        if (Info is { }) yield return Info;
        if (Encrypt is IPdfObject encrypt) yield return encrypt;
    }

    public static byte[] GenerateID() => Guid.NewGuid().ToByteArray();
}

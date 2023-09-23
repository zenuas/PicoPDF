using PicoPDF.Pdf.Element;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace PicoPDF.Pdf;

public class PdfObject : IPdfObject
{
    public int IndirectIndex { get; set; }
    public Dictionary<string, ElementValue> Elements { get; init; } = new();
    public MemoryStream? Stream { get; set; }
    public List<PdfObject> RelatedObjects { get; init; } = new();

    public virtual void DoExport(PdfExportOption option)
    {
    }

    public Stream GetWriteStream(bool deflate = true)
    {
        if (deflate)
        {
            _ = Elements.TryAdd("Filter", $"/FlateDecode");
            return new ZLibStream(Stream = new MemoryStream(), CompressionLevel.SmallestSize, true);
        }
        else
        {
            _ = Elements.Remove("Filter");
            return Stream = new MemoryStream();
        }
    }
}

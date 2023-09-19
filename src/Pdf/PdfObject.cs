using PicoPDF.Pdf.Element;
using System.Collections.Generic;
using System.IO;

namespace PicoPDF.Pdf;

public class PdfObject : IPdfObject
{
    public int IndirectIndex { get; set; }
    public Dictionary<string, ElementValue> Elements { get; init; } = new();
    public MemoryStream Stream { get; init; } = new();
    public List<PdfObject> RelatedObjects { get; init; } = new();

    public virtual void DoExport()
    {
    }
}

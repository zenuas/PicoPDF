using Mina.Extension;
using Pdf.Documents;
using Pdf.Elements;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace Pdf;

public class PdfValueObject<T> : IPdfObject where T : ElementValue
{
    public int IndirectIndex { get; set; }
    public required T Element { get; init; }

    public virtual void BeforeExport(PdfExportOption option)
    {
    }

    public virtual IEnumerable<IHaveReferences> GetReferences()
    {
        if (Element is IHaveReferences v)
        {
            yield return v;
        }
    }

    public void Export(Document document, Stream stream, PdfExportOption option)
    {
        stream.Write($"{IndirectIndex} 0 obj\n");
        using var converter = document.Encrypt?.StringHandler?.CreateEncrypterConverter(IndirectIndex, 0);
        stream.Write(Element.ToElementString(converter));
        stream.Write("endobj\n\n");
    }
}

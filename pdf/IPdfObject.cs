using Pdf.Documents;
using System.IO;

namespace Pdf;

public interface IPdfObject : IHaveReferences
{
    public int IndirectIndex { get; set; }

    public void BeforeExport(PdfExportOption option);
    public void Export(Document document, Stream stream, PdfExportOption option);
}

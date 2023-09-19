using Extensions;
using System.Collections.Generic;
using System.IO;

namespace PicoPDF.Pdf;

public static class PdfExport
{
    public static void Export(Document doc, Stream stream)
    {
        stream.Write($"%PDF-{doc.Version / 10}.{doc.Version % 10}\n");

        var xref = new List<long>();
        GetAllReferences(doc).Each((x, i) =>
        {
            xref.Add(stream.Position);
            stream.Write($"{x.IndirectIndex} 0 obj\n");
            stream.Write($"<<\n");
            x.Elements.Each(x => stream.Write($"  /{x.Key} {x.Value.ToElementString()}\n"));
            stream.Write($">>\n");
            if (x.Stream.Length > 0)
            {
                stream.Write($"stream\n");
                stream.Write(x.Stream.ToArray());
                stream.Write($"\nendstream\n");
            }
            stream.Write($"endobj\n");
            stream.Write($"\n");
        });

        var startxref = stream.Position;
        stream.Write($"xref\n");
        stream.Write($"0 {xref.Count + 1}\n");
        stream.Write($"0000000000 65535 f\n");
        xref.Each(x => stream.Write($"{x:0000000000} 00000 n\n"));
        stream.Write($"\n");

        stream.Write($"trailer\n");
        stream.Write($"<<\n");
        stream.Write($"  /Size {xref.Count + 1}\n");
        stream.Write($"  /Root {doc.Catalog.IndirectIndex} 0 R\n");
        stream.Write($">>\n");
        stream.Write($"startxref\n");
        stream.Write($"{startxref}\n");
        stream.Write($"%%EOF\n");
    }

    public static List<PdfObject> GetAllReferences(Document doc)
    {
        var refs = new List<PdfObject>();

        void refsadd(PdfObject x)
        {
            x.IndirectIndex = refs.Count + 1;
            x.DoExport();
            refs.Add(x);
            x.RelatedObjects.Each(refsadd);
        }
        doc.PdfObjects.Each(refsadd);

        return refs;
    }
}

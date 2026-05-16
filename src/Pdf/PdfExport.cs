using Mina.Extension;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Font;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf;

public static class PdfExport
{
    public static void Export(Document doc, Stream stream, PdfExportOption option)
    {
        stream.Write($"%PDF-{doc.Version / 10}.{doc.Version % 10}\n");
        stream.Write("%🍣\n\n"u8);

        foreach (var font in doc.PdfObjects.OfType<Type0Font>())
        {
            if (font.Chars.Count > 0 &&
                (font.FontEmbed == FontEmbed.ForceEmbed ||
                (font.FontEmbed == FontEmbed.PossibleEmbed && ((font.Font.OS2?.FsType ?? 0) & 0x2) == 0))) font.CreateEmbeddedFont();
        }
        var xref = new List<long>();
        GetAllReferences(doc, option).Where(x => x is not Type0Font font || font.Chars.Count > 0).Each((x, i) =>
        {
            xref.Add(stream.Position);
            stream.Write($"{x.IndirectIndex} 0 obj\n");
            stream.Write("<<\n");
            var input = x.Stream;
            if (input is { }) x.Elements["Length"] = input.Length;
            x.Elements.Each(x => stream.Write($"  /{x.Key} {x.Value.ToElementString()}\n"));
            stream.Write(">>\n");
            if (input is { })
            {
                stream.Write("stream\n");
                stream.Write(input.ToArray());
                stream.Write("\nendstream\n");
            }
            stream.Write("endobj\n\n");
        });

        var startxref = stream.Position;
        if (option.OutputCrossReferenceTable)
        {
            stream.Write("xref\n");
            stream.Write($"0 {xref.Count + 1}\n");
            stream.Write("0000000000 65535 f\r\n");
            xref.Each(x => stream.Write($"{x:0000000000} 00000 n\r\n"));
            stream.Write("\n");
        }

        stream.Write("trailer\n");
        stream.Write("<<\n");
        stream.Write($"  /Size {xref.Count + 1}\n");
        stream.Write($"  /Root {doc.Catalog.IndirectIndex} 0 R\n");
        if (doc.Info is { }) stream.Write($"  /Info {doc.Info.IndirectIndex} 0 R\n");
        stream.Write(">>\n");
        if (option.OutputCrossReferenceTable)
        {
            stream.Write("startxref\n");
            stream.Write($"{startxref}\n");
        }
        stream.Write("%%EOF\n");
    }

    public static PdfObject[] GetAllReferences(Document doc, PdfExportOption option)
    {
        var refs = new List<PdfObject>();

        void refsadd(PdfObject x)
        {
            x.IndirectIndex = refs.Count + 1;
            x.DoExport(option);
            refs.Add(x);
            x.RelatedObjects.Each(refsadd);
        }
        doc.PdfObjects.Each(refsadd);

        return [.. refs];
    }
}

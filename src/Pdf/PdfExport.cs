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
    public static void Export(Document document, Stream stream, PdfExportOption option)
    {
        stream.Write($"%PDF-{document.Version / 10}.{document.Version % 10}\n");
        stream.Write("%\U0001F363\n\n"u8);

        foreach (var font in document.PdfObjects.OfType<Type0Font>())
        {
            if (font.Chars.Count > 0 &&
                (font.FontEmbed == FontEmbeds.ForceEmbed ||
                (font.FontEmbed == FontEmbeds.PossibleEmbed && ((font.Font.OS2?.FsType ?? 0) & 0x2) == 0))) font.CreateEmbeddedFont();
        }
        var xref = new List<long>();
        GetAllReferences(document, option).Where(x => x is not Type0Font font || font.Chars.Count > 0).Each((x, i) =>
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
            // Each entry is exactly 20 bytes long, including the end-of-line marker.
            //   nnnnnnnnnn ggggg n eol
            //   nnnnnnnnnn ggggg f eol
            // where
            //   nnnnnnnnnn is a 10-digit byte offset
            //   ggggg is a 5-digit generation number
            //   n is a literal keyword identifying this as an in-use entry
            //   f is a literal keyword identifying this as a free entry
            //   eol is a 2-character end-of-line sequence
            stream.Write("0000000000 65535 f\r\n");
            xref.Each(x => stream.Write($"{x:0000000000} 00000 n\r\n"));
            stream.Write("\n");
        }

        stream.Write("trailer\n");
        stream.Write("<<\n");
        stream.Write($"  /Size {xref.Count + 1}\n");
        stream.Write($"  /Root {document.Catalog.IndirectIndex} 0 R\n");
        if (document.Info is { }) stream.Write($"  /Info {document.Info.IndirectIndex} 0 R\n");
        if (document.Encrypt is { }) stream.Write($"  /Encrypt {document.Encrypt.IndirectIndex} 0 R\n");
        stream.Write(">>\n");
        if (option.OutputCrossReferenceTable)
        {
            stream.Write("startxref\n");
            stream.Write($"{startxref}\n");
        }
        stream.Write("%%EOF\n");
    }

    public static PdfObject[] GetAllReferences(Document document, PdfExportOption option)
    {
        var refs = new List<PdfObject>();

        void refsadd(PdfObject x)
        {
            x.IndirectIndex = refs.Count + 1;
            x.DoExport(option);
            refs.Add(x);
            x.RelatedObjects.Each(refsadd);
        }
        document.PdfObjects.Each(refsadd);

        return [.. refs];
    }
}

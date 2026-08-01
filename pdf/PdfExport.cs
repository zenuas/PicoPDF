using Mina.Extension;
using Pdf.Documents;
using Pdf.Extension;
using Pdf.Font;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pdf;

public static class PdfExport
{
    public static void Export(Document document, Stream stream, PdfExportOption option)
    {
        stream.Write($"%PDF-{document.Version / 10}.{document.Version % 10}\n");
        // If a PDF file contains binary data, as most do, it is recommended that the header line be immediately followed by a comment line containing at least four binary characters—that is, characters whose codes are 128 or greater.
        // This ensures proper behavior of file transfer applications that inspect data near the beginning of a file to determine whether to treat the file's contents as text or as binary.
        stream.Write("%\U0001F363\n\n"u8);

        foreach (var font in document.Fonts.OfType<Type0Font>())
        {
            if (font.Chars.Count > 0 &&
                ((font.FontEmbed & FontEmbeds.EmbedsMask) == FontEmbeds.ForceEmbed ||
                ((font.FontEmbed & FontEmbeds.EmbedsMask) == FontEmbeds.PossibleEmbed && ((font.Font.OS2?.FsType ?? 0) & 0x2) == 0))) font.CreateEmbeddedFont();
        }

        var export_refs = GetAllReferencesExport(document, option, [.. document.Fonts.OfType<Type0Font>().Where(x => x is Type0Font font && font.Chars.Count == 0)]);
        export_refs.Each((x, i) => x.IndirectIndex = i + 1);

        var xref = new List<long>();
        export_refs.Each(pdfobj =>
        {
            xref.Add(stream.Position);
            pdfobj.Export(document, stream, option);
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
        if (document.Encrypt is { }) stream.Write($"  /Encrypt {document.Encrypt.Cast<PdfObject>().IndirectIndex} 0 R\n");
        if (document.DocumentID is { } id) stream.Write($"  /ID [{id.CreateID.ToHexString()} {id.UpdateID.ToHexString()}]\n");
        stream.Write(">>\n");
        if (option.OutputCrossReferenceTable)
        {
            stream.Write("startxref\n");
            stream.Write($"{startxref}\n");
        }
        stream.Write("%%EOF\n");
    }

    public static IPdfObject[] GetAllReferencesExport(Document document, PdfExportOption option, HashSet<IHaveReferences>? excludes = null)
    {
        var cache = excludes ?? [];

        return [.. document.GetReferences()
            .Select(x => TraverseReferences(cache, x, option))
            .Flatten()];
    }

    public static IEnumerable<IPdfObject> TraverseReferences(HashSet<IHaveReferences> cache, IHaveReferences reference, PdfExportOption option)
    {
        if (!cache.Add(reference)) yield break;
        if (reference is IPdfObject pdfobj)
        {
            pdfobj.BeforeExport(option);
            yield return pdfobj;
        }

        foreach (var x in reference.GetReferences())
        {
            foreach (var y in TraverseReferences(cache, x, option))
            {
                yield return y;
            }
        }
    }
}

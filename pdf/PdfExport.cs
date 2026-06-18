using Mina.Extension;
using Pdf.Documents;
using Pdf.Extension;
using Pdf.Font;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Pdf;

public static class PdfExport
{
    public static void Export(Document document, Stream stream, PdfExportOption option)
    {
        stream.Write($"%PDF-{document.Version / 10}.{document.Version % 10}\n");
        // If a PDF file contains binary data, as most do, it is recommended that the header line be immediately followed by a comment line containing at least four binary characters—that is, characters whose codes are 128 or greater.
        // This ensures proper behavior of file transfer applications that inspect data near the beginning of a file to determine whether to treat the file's contents as text or as binary.
        stream.Write("%\U0001F363\n\n"u8);

        foreach (var font in document.PdfObjects.OfType<Type0Font>())
        {
            if (font.Chars.Count > 0 &&
                (font.FontEmbed == FontEmbeds.ForceEmbed ||
                (font.FontEmbed == FontEmbeds.PossibleEmbed && ((font.Font.OS2?.FsType ?? 0) & 0x2) == 0))) font.CreateEmbeddedFont();
        }

        var all_refs = GetAllReferencesExport(document, option);
        var noglyphfont_refs = all_refs
            .Where(x => x is Type0Font font && font.Chars.Count == 0)
            .Select(GetAllReferences)
            .Flatten()
            .ToHashSet();
        var export_refs = all_refs
            .Where(x => !noglyphfont_refs.Contains(x))
            .ToArray();
        export_refs.Each((x, i) => x.IndirectIndex = i + 1);

        var xref = new List<long>();
        export_refs.Each(pdfobj =>
        {
            xref.Add(stream.Position);
            stream.Write($"{pdfobj.IndirectIndex} 0 obj\n");
            stream.Write("<<\n");
            var input = pdfobj.Stream;
            if (input is { })
            {
                var stream_pipe = document.StreamHandler?.CreateEncrypterPipe(pdfobj.IndirectIndex, 0);
                if (stream_pipe is { } p) input = EncryptStream(input, p.Input, p.Output).GetAwaiter().GetResult();
                pdfobj.Elements["Length"] = input.Length;
            }
            using var converter = document.StringHandler?.CreateEncrypterConverter(pdfobj.IndirectIndex, 0);
            pdfobj.Elements.Each(x => stream.Write($"  /{x.Key} {x.Value.ToElementString(converter)}\n"));
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
        if (document.DocumentID is { } id) stream.Write($"  /ID [{id.CreateID.ToHexString()} {id.UpdateID.ToHexString()}]\n");
        stream.Write(">>\n");
        if (option.OutputCrossReferenceTable)
        {
            stream.Write("startxref\n");
            stream.Write($"{startxref}\n");
        }
        stream.Write("%%EOF\n");
    }

    public static async Task<MemoryStream> EncryptStream(MemoryStream stream, PipeWriter input, PipeReader output)
    {
        stream.Position = 0;

        Span<byte> buffer = stackalloc byte[4096];
        var writer = new MemoryStream();
        while (true)
        {
            var readed = stream.Read(buffer);
            if (readed == 0) break;

            input.Write(buffer[..readed]);
        }
        input.Complete();
        while (true)
        {
            var result = await output.ReadAsync();
            if (result.IsCanceled) throw new OperationCanceledException();
            if (result.Buffer.IsEmpty) break;

            writer.Write(result.Buffer.ToArray());
            output.AdvanceTo(result.Buffer.End);
        }
        return writer;
    }

    public static PdfObject[] GetAllReferencesExport(Document document, PdfExportOption option)
    {
        var refs = new List<PdfObject>();

        void refsadd(PdfObject x)
        {
            x.DoExport(option);
            refs.Add(x);
            x.RelatedObjects.Each(refsadd);
        }
        document.PdfObjects.Each(refsadd);

        return [.. refs];
    }

    public static PdfObject[] GetAllReferences(PdfObject pdfobj)
    {
        var refs = new List<PdfObject>();

        void refsadd(PdfObject x)
        {
            refs.Add(x);
            x.RelatedObjects.Each(refsadd);
        }
        refsadd(pdfobj);

        return [.. refs];
    }
}

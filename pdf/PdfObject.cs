using Mina.Extension;
using Pdf.Documents;
using Pdf.Elements;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Pdf;

public class PdfObject : IPdfObject
{
    public int IndirectIndex { get; set; }
    public Dictionary<string, ElementValue> Elements { get; init; } = [];
    public MemoryStream? Stream { get; set; }

    public virtual void BeforeExport(PdfExportOption option)
    {
    }

    public virtual IEnumerable<IHaveReferences> GetReferences()
    {
        foreach (var v in Elements.Values.OfType<IHaveReferences>())
        {
            yield return v;
        }
    }

    public Stream GetWriteStream(bool deflate = true)
    {
        if (deflate)
        {
            _ = Elements.TryAdd("Filter", "/FlateDecode");
            return new ZLibStream(Stream = new(), CompressionLevel.SmallestSize, true);
        }
        else
        {
            _ = Elements.Remove("Filter");
            return Stream = new();
        }
    }

    public void Export(Document document, Stream stream, PdfExportOption option)
    {
        stream.Write($"{IndirectIndex} 0 obj\n");
        stream.Write("<<\n");
        if (Stream is { })
        {
            var stream_pipe = (this is not IMetadata || (document.Encrypt?.MetadataEncrypted ?? false) ? document.Encrypt?.StreamHandler : null)?.CreateEncrypterPipe(IndirectIndex, 0);
            if (stream_pipe is { } p) Stream = EncryptStream(Stream, p.Input, p.Output).GetAwaiter().GetResult();
            Elements["Length"] = Stream.Length;
        }
        using var converter = document.Encrypt?.StringHandler?.CreateEncrypterConverter(IndirectIndex, 0);
        Elements.Each(x => stream.Write($"  /{x.Key} {x.Value.ToElementString(converter)}\n"));
        stream.Write(">>\n");
        if (Stream is { })
        {
            stream.Write("stream\n");
            stream.Write(Stream.ToArray());
            stream.Write("\nendstream\n");
        }
        stream.Write("endobj\n\n");
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
}

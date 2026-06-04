using System;

namespace PicoPDF.Pdf.Documents.Security;

public interface IFilter : IDisposable
{
    public byte[] Filter(ReadOnlySpan<byte> data);
}
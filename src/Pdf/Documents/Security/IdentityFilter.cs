using System;

namespace PicoPDF.Pdf.Documents.Security;

public class IdentityFilter : IFilter
{
    public byte[] Filter(ReadOnlySpan<byte> data) => [.. data];
    public byte[] FilterFinal(ReadOnlySpan<byte> data) => [.. data];
    public void Dispose() => GC.SuppressFinalize(this);
}

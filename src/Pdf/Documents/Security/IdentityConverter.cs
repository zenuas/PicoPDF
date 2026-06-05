using System;

namespace PicoPDF.Pdf.Documents.Security;

public class IdentityConverter : IConverter
{
    public byte[] Convert(ReadOnlySpan<byte> data) => [.. data];

    public void Dispose() => GC.SuppressFinalize(this);
}

using System;

namespace Pdf.Documents.Security;

public interface IConverter : IDisposable
{
    public byte[] Convert(ReadOnlySpan<byte> data);
}

using System;

namespace PicoPDF.Pdf.Documents.Security;

public interface IConverter : IDisposable
{
    public byte[] Convert(ReadOnlySpan<byte> data);
}

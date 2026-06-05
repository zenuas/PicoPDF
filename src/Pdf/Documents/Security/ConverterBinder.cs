using System;

namespace PicoPDF.Pdf.Documents.Security;

public class ConverterBinder : IConverter
{
    public required Func<ReadOnlySpan<byte>, byte[]> Convert { get; init; }
    public required Action Dispose { get; init; }

    byte[] IConverter.Convert(ReadOnlySpan<byte> data) => Convert(data);

    void IDisposable.Dispose()
    {
        Dispose();
        GC.SuppressFinalize(this);
    }
}

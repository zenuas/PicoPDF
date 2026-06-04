using System;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public class Aes128EncryptFilter : IFilter
{
    public required ICryptoTransform Encryptor { get; init; }
    public required byte[]? IV { get; set; }

    public byte[] Filter(ReadOnlySpan<byte> data)
    {
        if (IV is { } iv)
        {
            IV = null;
            return [.. iv, .. data.Length > 0 ? Encryptor.TransformFinalBlock([.. data], 0, data.Length) : []];
        }

        return Encryptor.TransformFinalBlock([.. data], 0, data.Length);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Encryptor.Dispose();
    }
}

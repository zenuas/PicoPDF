using System;
using System.IO;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public class Aes128EncryptFilter : IFilter
{
    public required ICryptoTransform Encryptor { get; init; }
    public required byte[] IV { get; init; }
    public MemoryStream MemoryStream { get; init; } = new();
    public CryptoStream? CryptoStream { get; set; }

    public byte[] Filter(ReadOnlySpan<byte> data)
    {
        if (CryptoStream is null)
        {
            MemoryStream.Write(IV);
            CryptoStream = new CryptoStream(MemoryStream, Encryptor, CryptoStreamMode.Write);
        }

        if (data.Length > 0) CryptoStream.Write(data);
        return [];
    }

    public byte[] FilterFinal(ReadOnlySpan<byte> data)
    {
        _ = Filter(data);
        CryptoStream!.FlushFinalBlock();
        return MemoryStream.ToArray();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Encryptor.Dispose();
        CryptoStream?.Dispose();
        MemoryStream.Dispose();
    }
}

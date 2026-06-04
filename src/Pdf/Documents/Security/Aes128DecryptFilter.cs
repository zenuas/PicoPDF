using System;
using System.IO;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public class Aes128DecryptFilter : IFilter
{
    public required Aes Cipher { get; init; }
    public ICryptoTransform? Decryptor { get; set; }
    public byte[] IV { get; init; } = new byte[16];
    public int IVReaded { get; set; } = 0;
    public MemoryStream MemoryStream { get; init; } = new();
    public CryptoStream? CryptoStream { get; set; }

    public int ReadIV(ReadOnlySpan<byte> data)
    {
        // The block size parameter is set to 16 bytes, and the initialization vector is a 16-byte random number that is stored as the first 16 bytes of the encrypted stream or string.
        var readed = Math.Min(16 - IVReaded, data.Length);
        data[0..readed].CopyTo(IV.AsSpan(IVReaded));
        IVReaded += readed;
        if (IVReaded < 16) return readed;
        Cipher.IV = IV;
        CryptoStream = new CryptoStream(MemoryStream, Decryptor = Cipher.CreateDecryptor(), CryptoStreamMode.Write);
        return readed;
    }

    public byte[] Filter(ReadOnlySpan<byte> data)
    {
        if (IVReaded < 16)
        {
            var readed = ReadIV(data);
            data = data[readed..];
            if (data.Length == 0) return [];
        }
        if (data.Length > 0) CryptoStream!.Write(data);
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
        Cipher.Dispose();
        Decryptor?.Dispose();
        CryptoStream?.Dispose();
        MemoryStream.Dispose();
    }
}

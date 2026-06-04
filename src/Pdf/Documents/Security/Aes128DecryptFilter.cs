using System;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public class Aes128DecryptFilter : IFilter
{
    public required Aes Cipher { get; init; }
    public ICryptoTransform? Decryptor { get; set; }
    public byte[] IV { get; init; } = new byte[16];
    public int IVReaded { get; set; } = 0;

    public byte[] Filter(ReadOnlySpan<byte> data)
    {
        if (IVReaded < 16)
        {
            // The block size parameter is set to 16 bytes, and the initialization vector is a 16-byte random number that is stored as the first 16 bytes of the encrypted stream or string.
            var readed = Math.Min(16 - IVReaded, data.Length);
            data[0..readed].CopyTo(IV.AsSpan(IVReaded));
            IVReaded += readed;
            if (IVReaded < 16) return [];
            data = data[readed..];
            Cipher.IV = IV;
            Decryptor = Cipher.CreateDecryptor();
        }
        return Decryptor!.TransformFinalBlock([.. data], 0, data.Length);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Cipher.Dispose();
        Decryptor?.Dispose();
    }
}

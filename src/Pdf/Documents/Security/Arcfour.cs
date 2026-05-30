using System;

namespace PicoPDF.Pdf.Documents.Security;

public static class Arcfour
{
    public static byte[] InitializeKey(Span<byte> key)
    {
        var s = new byte[256];
        for (var i = 0; i < 256; i++) s[i] = (byte)i;

        for (int i = 0, i2 = 0, k = 0; i < 256; i++, k = (k + 1) % key.Length)
        {
            i2 = (key[k] + s[i] + i2) & 0xFF;
            (s[i], s[i2]) = (s[i2], s[i]);
        }

        return s;
    }

    public static (int X, int Y) Encrypt(byte[] key_state, Span<byte> data, int x = 0, int y = 0) => Encrypt(key_state, data, data, x, y);
    public static (int X, int Y) Encrypt(byte[] key_state, ReadOnlySpan<byte> data, Span<byte> outdata, int x = 0, int y = 0)
    {
        for (int i = 0; i < data.Length; i++)
        {
            x = (x + 1) & 0xFF;
            y = (key_state[x] + y) & 0xFF;

            (key_state[x], key_state[y]) = (key_state[y], key_state[x]);
            outdata[i] = (byte)(data[i] ^ key_state[(key_state[x] + key_state[y]) & 0xFF]);
        }
        return (x, y);
    }
}

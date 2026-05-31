using System;

namespace PicoPDF.Pdf.Documents.Security;

public static class Arcfour
{
    public static byte[] InitializeKey(Span<byte> key)
    {
        var state = new byte[256];
        for (var i = 0; i < state.Length; i++) state[i] = (byte)i;

        for (int i = 0, prev = 0; i < state.Length; i++)
        {
            prev = (key[i % key.Length] + state[i] + prev) & 0xFF;
            (state[i], state[prev]) = (state[prev], state[i]);
        }
        return state;
    }

    public static (int X, int Y) Encrypt(byte[] state, Span<byte> data, int x = 0, int y = 0) => Encrypt(state, data, data, x, y);
    public static (int X, int Y) Encrypt(byte[] state, ReadOnlySpan<byte> data, Span<byte> outdata, int x = 0, int y = 0)
    {
        for (int i = 0; i < data.Length; i++)
        {
            x = (x + 1) & 0xFF;
            y = (state[x] + y) & 0xFF;

            (state[x], state[y]) = (state[y], state[x]);
            outdata[i] = (byte)(data[i] ^ state[(state[x] + state[y]) & 0xFF]);
        }
        return (x, y);
    }
}

namespace PicoPDF.Pdf.Documents.Security;

public static class Arcfour
{
    public static byte[] InitializeKey(byte[] key) => InitializeKey(key, 0, key.Length);
    public static byte[] InitializeKey(byte[] key, int offset, int length)
    {
        var s = new byte[256];
        for (var i = 0; i < 256; i++) s[i] = (byte)i;

        for (int i = 0, i2 = 0, k = 0; i < 256; i++, k = (k + 1) % length)
        {
            i2 = (key[k + offset] + s[i] + i2) & 0xFF;
            (s[i], s[i2]) = (s[i2], s[i]);
        }

        return s;
    }

    public static (int X, int Y) Encrypt(byte[] key_state, byte[] data) => Encrypt(key_state, data, 0, data.Length, data, 0, 0, 0);
    public static (int X, int Y) Encrypt(byte[] key_state, byte[] data, int offset, int length, int x = 0, int y = 0) => Encrypt(key_state, data, offset, length, data, offset, x, y);
    public static (int X, int Y) Encrypt(byte[] key_state, byte[] data, byte[] outdata) => Encrypt(key_state, data, 0, data.Length, outdata, 0, 0, 0);
    public static (int X, int Y) Encrypt(byte[] key_state, byte[] data, int offset, int length, byte[] outdata) => Encrypt(key_state, data, offset, length, outdata, 0, 0, 0);
    public static (int X, int Y) Encrypt(byte[] key_state, byte[] data, int offset, int length, byte[] outdata, int outoffset, int x = 0, int y = 0)
    {
        var total_length = offset + length;
        for (int i = offset; i < total_length; i++)
        {
            x = (x + 1) & 0xFF;
            y = (key_state[x] + y) & 0xFF;

            (key_state[x], key_state[y]) = (key_state[y], key_state[x]);
            outdata[i + outoffset - offset] = (byte)(data[i] ^ key_state[(key_state[x] + key_state[y]) & 0xFF]);
        }
        return (x, y);
    }
}

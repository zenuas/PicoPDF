using System.Collections.Generic;
using System.IO;

namespace Extensions;

public static class Streams
{
    public static void Write(this Stream self, string s)
    {
        self.Write(System.Text.Encoding.UTF8.GetBytes(s));
    }

    public static IEnumerable<byte> ReadAllBytes(this Stream self, int buffer_size = 1024)
    {
        var buffer = new byte[buffer_size];
        while (true)
        {
            var readed = self.Read(buffer, 0, buffer_size);
            if (readed <= 0) break;
            for (int i = 0; i < readed; i++)
            {
                yield return buffer[i];
            }
        }
    }
}

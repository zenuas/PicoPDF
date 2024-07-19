using Mina.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class DictData : Dictionary<int, IntOrDouble[]>
{
    public static DictData ReadFrom(byte[] bytes)
    {
        var kv = new DictData();
        var values = new List<IntOrDouble>();
        for (var i = 0; i < bytes.Length; i++)
        {
            var b0 = bytes[i];

            if (b0 is 12)
            {
                var b1 = bytes[++i];
                kv.Add((b0 * 100) + b1, [.. values]);
                values.Clear();
            }
            else if (b0 is >= 0 and <= 21)
            {
                kv.Add(b0, [.. values]);
                values.Clear();
            }
            else if (b0 is 28 or 29 or (>= 32 and <= 254))
            {
                values.Add(ReadDictDataNumber(b0, bytes[(i + 1)..].AsSpan()));
                i += DictDataNumberSize(b0);
            }
            else if (b0 is 30)
            {
                values.Add(PackedBCDToDouble(Lists.Repeat(0)
                        .Select(_ => bytes[++i])
                        .Select(x => new byte[] { (byte)(x >> 4), (byte)(x & 0x0f) })
                        .Flatten()
                        .TakeWhile(x => x != 0x0f)
                        .ToArray()
                    ));
            }
        }
        return kv;
    }

    public static int ReadDictDataNumber(byte b0, Span<byte> bytes) => b0 switch
    {
        >= 32 and <= 246 => b0 - 139,
        >= 247 and <= 250 => ((b0 - 247) * 256) + bytes[0] + 108,
        >= 251 and <= 254 => (-(b0 - 251) * 256) - bytes[0] - 108,
        28 => (short)((bytes[0] << 8) | bytes[1]),
        29 => (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3],
        _ => 0,
    };

    public static int DictDataNumberSize(byte b0) => b0 switch
    {
        >= 32 and <= 246 => 0,
        >= 247 and <= 250 => 1,
        >= 251 and <= 254 => 1,
        28 => 2,
        29 => 4,
        _ => 0,
    };

    public static double PackedBCDToDouble(byte[] bytes)
    {
        var value = 0d;
        var i = 0;
        var sign = true;

        if (i < bytes.Length && bytes[i] == 0x0e)
        {
            sign = false;
            i++;
        }

        for (; i < bytes.Length && bytes[i] <= 9; i++)
        {
            value = (value * 10) + bytes[i];
        }

        if (i < bytes.Length && bytes[i] == 0x0a)
        {
            i++;
            var dec = 0.1;
            for (; i < bytes.Length && bytes[i] <= 9; i++, dec /= 10)
            {
                value += bytes[i] * dec;
            }
        }

        if (i < bytes.Length && bytes[i] is 0x0b or 0x0c)
        {
            var esign = bytes[i] == 0x0b;
            i++;
            var e = 0;
            for (; i < bytes.Length && bytes[i] <= 9; i++)
            {
                e = (e * 10) + bytes[i];
            }
            value *= Math.Pow(10, esign ? e : -e);
        }

        return sign ? value : -value;
    }
}

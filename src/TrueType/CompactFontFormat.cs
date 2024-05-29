using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.TrueType;

public class CompactFontFormat(Stream stream)
{
    public readonly byte Major = (byte)stream.ReadByte();
    public readonly byte Minor = (byte)stream.ReadByte();
    public readonly byte HdrSize = (byte)stream.ReadByte();
    public readonly byte OffSize = (byte)stream.ReadByte();

    public readonly string[] NameIndex = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
    public readonly Dictionary<string, object>[] TopDictIndex = ReadIndexData(stream).Select(x => ReadDictData(new MemoryStream(x), x.Length)).ToArray();
    public readonly string[] StringIndex = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();

    public static byte[][] ReadIndexData(Stream stream)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var offSize = (byte)stream.ReadByte();

        Func<Stream, int> offsetRead = offSize switch
        {
            1 => (x) => x.ReadByte(),
            2 => (x) => BinaryPrimitives.ReadUInt16BigEndian(x.ReadBytes(2)),
            3 => (x) => (int)BinaryPrimitives.ReadUInt32BigEndian(x.ReadBytes(3)),
            _ => (x) => (int)BinaryPrimitives.ReadUInt32BigEndian(x.ReadBytes(4)),
        };

        var offset = Lists.RangeTo(0, count).Select(_ => offsetRead(stream)).ToArray();
        return Lists.RangeTo(0, count - 1).Select(i => stream.ReadBytes(offset[i + 1] - offset[i])).ToArray();
    }

    public static Dictionary<string, object> ReadDictData(Stream stream, int length)
    {
        var kv = new Dictionary<string, object>();
        var values = new List<object>();
        var pos = stream.Position;
        while (stream.Position < pos + length)
        {
            var b0 = (byte)stream.ReadByte();

            if (b0 is 12)
            {
                var b1 = (byte)stream.ReadByte();
                kv.Add($"{b0} {b1}", values.ToArray());
                values.Clear();
            }
            else if (b0 is >= 0 and <= 21)
            {
                kv.Add($"{b0}", values.ToArray());
                values.Clear();
            }
            else if (b0 is 28 or 29 or (>= 32 and <= 254))
            {
                values.Add(b0 switch
                {
                    >= 32 and <= 246 => b0 - 139,
                    >= 247 and <= 250 => ((b0 - 247) * 256) + stream.ReadByte() + 108,
                    >= 251 and <= 254 => (-(b0 - 251) * 256) + stream.ReadByte() - 108,
                    28 => (stream.ReadByte() << 8) | stream.ReadByte(),
                    29 => (stream.ReadByte() << 24) | (stream.ReadByte() << 16) | (stream.ReadByte() << 8) | stream.ReadByte(),
                    _ => 0,
                });
            }
            else if (b0 is 30)
            {
                var value = PackedBCDToDouble(Lists.Repeat(stream)
                        .Select(x => (byte)x.ReadByte())
                        .Select(x => new byte[] { (byte)(x >> 4), (byte)(x & 0x0f) })
                        .Flatten()
                        .TakeWhile(x => x != 0x0f)
                        .ToArray()
                    );
                values.Add(value);
            }
        }
        return kv;
    }

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

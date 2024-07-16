using Mina.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CompactFontFormat : IExportable
{
    public required byte Major { get; init; }
    public required byte Minor { get; init; }
    public required byte HeaderSize { get; init; }
    public required byte OffsetSize { get; init; }
    public required string[] Names { get; init; }
    public required Dictionary<int, IntOrDouble[]> TopDict { get; init; }
    public required string[] Strings { get; init; }
    public required byte[][] GlobalSubroutines { get; init; }
    public required byte[][] CharStrings { get; init; }
    public required Charsets Charsets { get; init; }
    public required Dictionary<int, IntOrDouble[]> PrivateDict { get; init; }
    public required Dictionary<int, IntOrDouble[]>[] FontDictArray { get; init; }
    public required byte[] FontDictSelect { get; init; }

    public static CompactFontFormat ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major = stream.ReadUByte();
        var minor = stream.ReadUByte();
        var header_size = stream.ReadUByte();
        var offset_size = stream.ReadUByte();
        var names = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var top_dict = ReadIndexData(stream).Select(x => ReadDictData(new MemoryStream(x), x.Length)).First();
        var strings = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var global_subr = ReadIndexData(stream);

        var charset_offset = top_dict.TryGetValue(15, out var xs1) ? xs1[0].ToInt() : 0;
        var char_strings_offset = top_dict.TryGetValue(17, out var xs2) ? xs2[0].ToInt() : 0;
        var (private_dict_size, private_dict_offset) = top_dict.TryGetValue(18, out var xs3) ? (xs3[0].ToInt(), xs3[1].ToInt()) : (0, 0);

        stream.Position = position + char_strings_offset;
        var char_strings = ReadIndexData(stream);

        stream.Position = position + charset_offset;
        var charsets = ReadCharsets(stream, char_strings.Length - 1);

        stream.Position = position + private_dict_offset;
        var private_dict = ReadDictData(new MemoryStream(stream.ReadBytes(private_dict_size)), private_dict_size);

        Dictionary<int, IntOrDouble[]>[]? fdarray = null;
        byte[]? fdselect = null;
        if (top_dict.ContainsKey(1230))
        {
            var fdarray_offset = top_dict.TryGetValue(1236, out var xs4) ? xs4[0].ToInt() : 0;
            var fdselect_offset = top_dict.TryGetValue(1237, out var xs5) ? xs5[0].ToInt() : 0;

            stream.Position = position + fdarray_offset;
            fdarray = ReadIndexData(stream).Select(x => ReadDictData(new MemoryStream(x), x.Length)).ToArray();

            stream.Position = position + fdselect_offset;
            fdselect = ReadFDSelect(stream, char_strings.Length);
        }

        return new()
        {
            Major = major,
            Minor = minor,
            HeaderSize = header_size,
            OffsetSize = offset_size,
            Names = names,
            TopDict = top_dict,
            Strings = strings,
            GlobalSubroutines = global_subr,
            CharStrings = char_strings,
            Charsets = charsets,
            PrivateDict = private_dict,
            FontDictArray = fdarray ?? [],
            FontDictSelect = fdselect ?? [],
        };
    }

    public static byte[][] ReadIndexData(Stream stream)
    {
        var count = stream.ReadUShortByBigEndian();
        if (count == 0) return [];
        var offset_size = stream.ReadUByte();

        Func<Stream, int> offset_read = offset_size switch
        {
            1 => (x) => x.ReadUByte(),
            2 => (x) => x.ReadUShortByBigEndian(),
            3 => (x) => (x.ReadUByte() << 16) | (x.ReadUByte() << 8) | x.ReadUByte(),
            _ => (x) => (int)x.ReadUIntByBigEndian(),
        };

        var offset = Enumerable.Repeat(0, count + 1).Select(_ => offset_read(stream)).ToArray();
        return Enumerable.Range(0, count).Select(i => stream.ReadBytes(offset[i + 1] - offset[i])).ToArray();
    }

    public static Dictionary<int, IntOrDouble[]> ReadDictData(Stream stream, int length)
    {
        var kv = new Dictionary<int, IntOrDouble[]>();
        var values = new List<IntOrDouble>();
        var pos = stream.Position;
        while (stream.Position < pos + length)
        {
            var b0 = stream.ReadUByte();

            if (b0 is 12)
            {
                var b1 = stream.ReadUByte();
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
                values.Add(ReadDictDataNumber(b0, stream));
            }
            else if (b0 is 30)
            {
                values.Add(PackedBCDToDouble(Lists.Repeat(stream)
                        .Select(x => x.ReadUByte())
                        .Select(x => new byte[] { (byte)(x >> 4), (byte)(x & 0x0f) })
                        .Flatten()
                        .TakeWhile(x => x != 0x0f)
                        .ToArray()
                    ));
            }
        }
        return kv;
    }

    public static byte[] DictDataTo5Bytes(Dictionary<int, IntOrDouble[]> kv)
    {
        using var mem = new MemoryStream();
        foreach (var (k, vs) in kv)
        {
            vs.Each(x => mem.Write(DictDataNumberTo5Bytes(x.ToInt())));
            mem.Write(k >= 100 ? [(byte)(k / 100), (byte)(k % 100)] : [(byte)k]);
        }
        return mem.ToArray();
    }

    public static int ReadDictDataNumber(byte b0, Stream stream) => b0 switch
    {
        >= 32 and <= 246 => b0 - 139,
        >= 247 and <= 250 => ((b0 - 247) * 256) + stream.ReadUByte() + 108,
        >= 251 and <= 254 => (-(b0 - 251) * 256) - stream.ReadUByte() - 108,
        28 => (short)((stream.ReadUByte() << 8) | stream.ReadUByte()),
        29 => (stream.ReadUByte() << 24) | (stream.ReadUByte() << 16) | (stream.ReadUByte() << 8) | stream.ReadUByte(),
        _ => 0,
    };

    public static byte[] DictDataNumberToBytes(int number) =>
        number is >= -107 and <= 107 ? ([(byte)(number + 139)])
        : number is >= 108 and <= 1131 ? ([(byte)((((number - 108) >> 8) & 0xFF) + 247), (byte)(number - 108)])
        : number is >= -1131 and <= -108 ? ([(byte)(((-(number + 108) >> 8) & 0xFF) + 251), (byte)-(number + 108)])
        : number is >= -32768 and <= 32767 ? ([28, (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)])
        : DictDataNumberTo5Bytes(number);

    public static byte[] DictDataNumberTo5Bytes(int number) => ([29, (byte)((number >> 24) & 0xFF), (byte)((number >> 16) & 0xFF), (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)]);

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

    public static Charsets ReadCharsets(Stream stream, int glyph_count_without_notdef)
    {
        var format = stream.ReadUByte();
        if (format is 1 or 2)
        {
            var glyph = new List<ushort>();
            while (glyph.Count < glyph_count_without_notdef)
            {
                var first = stream.ReadUShortByBigEndian();
                var left = format == 1 ? stream.ReadUByte() : stream.ReadUShortByBigEndian();
                Enumerable.Range(0, left + 1).Each(x => glyph.Add((ushort)(first + x)));
            }

            return new()
            {
                Format = format,
                Glyph = [.. glyph],
            };
        }
        else
        {
            return new()
            {
                Format = 0,
                Glyph = Enumerable.Repeat(0, glyph_count_without_notdef).Select(_ => stream.ReadUShortByBigEndian()).ToArray(),
            };
        }
    }

    public static byte[] ReadFDSelect(Stream stream, int glyph_count_with_notdef)
    {
        var format = stream.ReadUByte();
        if (format == 3)
        {
            var fdselect = new List<byte>();
            var ranges = stream.ReadUShortByBigEndian();
            var first = stream.ReadUShortByBigEndian();
            for (var i = 0; i < ranges; i++)
            {
                var fd = stream.ReadUByte();
                var sentinel = stream.ReadUShortByBigEndian();
                Lists.RangeTo(first, sentinel - 1).Each(_ => fdselect.Add(fd));
                first = sentinel;
            }
            return [.. fdselect];
        }
        else
        {
            return Enumerable.Repeat(0, glyph_count_with_notdef).Select(_ => stream.ReadUByte()).ToArray();
        }
    }

    public void WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteByte(Major);
        stream.WriteByte(Minor);
        stream.WriteByte(HeaderSize);
        stream.WriteByte(OffsetSize);
        WriteIndexData(stream, Names.Select(Encoding.UTF8.GetBytes).ToArray());

        var top_dict_start = stream.Position;
        var top_dict = TopDict.ToDictionary();
        WriteIndexData(stream, [DictDataTo5Bytes(top_dict)]);
        WriteIndexData(stream, Strings.Select(Encoding.UTF8.GetBytes).ToArray());
        WriteIndexData(stream, GlobalSubroutines);

        top_dict[15] = [stream.Position - position];
        WriteCharsets(stream, Charsets);

        top_dict[17] = [stream.Position - position];
        WriteIndexData(stream, CharStrings);

        top_dict[18] = [0, stream.Position - position];
        var private_dict = DictDataTo5Bytes(PrivateDict);
        stream.Write(private_dict);
        top_dict[18][0] = private_dict.Length;

        if (top_dict.ContainsKey(1230))
        {
            top_dict[1237] = [stream.Position - position];
            WriteFDSelect(stream, FontDictSelect);
        }

        var lastposition = stream.Position;
        stream.Position = top_dict_start;
        WriteIndexData(stream, [DictDataTo5Bytes(top_dict)]);
        stream.Position = lastposition;
    }

    public static void WriteIndexData(Stream stream, byte[][] index)
    {
        stream.WriteUShortByBigEndian((ushort)index.Length);
        if (index.Length == 0) return;
        stream.WriteByte(4);
        index.Aggregate<byte[], uint[]>([1], (acc, x) => [.. acc, (uint)(acc.Last() + x.Length)]).Each(stream.WriteUIntByBigEndian);
        index.Each(x => stream.Write(x));
    }

    public static void WriteCharsets(Stream stream, Charsets charsets)
    {
        stream.WriteByte(0);
        charsets.Glyph.Each(stream.WriteUShortByBigEndian);
    }

    public static void WriteFDSelect(Stream stream, byte[] fdselect)
    {
        stream.WriteByte(0);
        fdselect.Each(stream.WriteByte);
    }
}

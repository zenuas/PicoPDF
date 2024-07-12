﻿using Mina.Extension;
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
    public required string[] NameIndex { get; init; }
    public required Dictionary<string, object[]> TopDictIndex { get; init; }
    public required string[] StringIndex { get; init; }
    public required byte[][] CharStrings { get; init; }
    public required ICharsets Charsets { get; init; }

    public static CompactFontFormat ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major = stream.ReadUByte();
        var minor = stream.ReadUByte();
        var header_size = stream.ReadUByte();
        var offset_size = stream.ReadUByte();
        var name_index = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var top_dict_index = ReadIndexData(stream).Select(x => ReadDictData(new MemoryStream(x), x.Length)).First();
        var string_index = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();

        var charset_offset = top_dict_index.TryGetValue("15", out var xs1) ? SafeConvert.ToLong(xs1[0], 0) : 0;
        var char_strings_offset = top_dict_index.TryGetValue("17", out var xs2) ? SafeConvert.ToLong(xs2[0], 0) : 0;

        stream.Position = position + char_strings_offset;
        var char_strings = ReadIndexData(stream);

        stream.Position = position + charset_offset;
        var charsets = ReadCharsets(stream, char_strings.Length);

        return new()
        {
            Major = major,
            Minor = minor,
            HeaderSize = header_size,
            OffsetSize = offset_size,
            NameIndex = name_index,
            TopDictIndex = top_dict_index,
            StringIndex = string_index,
            CharStrings = char_strings,
            Charsets = charsets,
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

        var offset = Enumerable.Range(0, count + 1).Select(_ => offset_read(stream)).ToArray();
        return Enumerable.Range(0, count).Select(i => stream.ReadBytes(offset[i + 1] - offset[i])).ToArray();
    }

    public static Dictionary<string, object[]> ReadDictData(Stream stream, int length)
    {
        var kv = new Dictionary<string, object[]>();
        var values = new List<object>();
        var pos = stream.Position;
        while (stream.Position < pos + length)
        {
            var b0 = stream.ReadUByte();

            if (b0 is 12)
            {
                var b1 = stream.ReadUByte();
                kv.Add($"{b0} {b1}", [.. values]);
                values.Clear();
            }
            else if (b0 is >= 0 and <= 21)
            {
                kv.Add($"{b0}", [.. values]);
                values.Clear();
            }
            else if (b0 is 28 or 29 or (>= 32 and <= 254))
            {
                values.Add(ReadDictDataNumber(b0, stream));
            }
            else if (b0 is 30)
            {
                var value = PackedBCDToDouble(Lists.Repeat(stream)
                        .Select(x => x.ReadUByte())
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

    public static byte[] DictDataTo5Bytes(Dictionary<string, object[]> kv)
    {
        using var mem = new MemoryStream();
        foreach (var (k, vs) in kv)
        {
            vs.OfType<int>().Each(x => mem.Write(DictDataNumberTo5Bytes(x)));
            var separator = k.IndexOf(' ');
            mem.Write(separator >= 0 ? [byte.Parse(k[0..separator]), byte.Parse(k[(separator + 1)..])] : [byte.Parse(k)]);
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

    public static ICharsets ReadCharsets(Stream stream, int glyph_count)
    {
        var format = stream.ReadUByte();
        return format switch
        {
            1 => CharsetsExpert.ReadFrom(stream, glyph_count - 1),
            2 => CharsetsExpertSubset.ReadFrom(stream, glyph_count - 1),
            _ => CharsetsISOAdobe.ReadFrom(stream, glyph_count - 1),
        };
    }

    public void WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteByte(Major);
        stream.WriteByte(Minor);
        stream.WriteByte(HeaderSize);
        stream.WriteByte(OffsetSize);
        WriteIndexData(stream, NameIndex.Select(Encoding.UTF8.GetBytes).ToArray());

        var top_dict_start = stream.Position;
        var top_dict = new Dictionary<string, object[]>
        {
            ["17"] = [0]
        };
        WriteIndexData(stream, [DictDataTo5Bytes(top_dict)]);
        WriteIndexData(stream, StringIndex.Select(Encoding.UTF8.GetBytes).ToArray());

        top_dict["17"] = [stream.Position - position];
        WriteIndexData(stream, CharStrings);

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
}

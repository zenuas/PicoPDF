using Mina.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenType.Tables.PostScript;

public class TopDict
{
    public required Dictionary<TopDictOperators, IntOrDouble[]> Dict { get; init; }
    public string[] Strings { get; init; } = [];
    public byte[][] CharStrings { get; init; } = [];
    public Charsets? Charsets { get; init; }
    public PrivateDict? PrivateDict { get; init; }
    public TopDict[] FontDictArray { get; init; } = [];
    public byte[] FontDictSelect { get; init; } = [];

    public string Version { get => Dict.TryGetValue(TopDictOperators.Version, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Notice { get => Dict.TryGetValue(TopDictOperators.Notice, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Copyright { get => Dict.TryGetValue(TopDictOperators.Copyright, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FullName { get => Dict.TryGetValue(TopDictOperators.FullName, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FamilyName { get => Dict.TryGetValue(TopDictOperators.FamilyName, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Weight { get => Dict.TryGetValue(TopDictOperators.Weight, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsFixedPitch { get => Dict.TryGetValue(TopDictOperators.IsFixedPitch, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble ItalicAngle { get => Dict.TryGetValue(TopDictOperators.ItalicAngle, out var xs) ? xs[0] : 0; }
    public IntOrDouble UnderlinePosition { get => Dict.TryGetValue(TopDictOperators.UnderlinePosition, out var xs) ? xs[0] : -100; }
    public IntOrDouble UnderlineThickness { get => Dict.TryGetValue(TopDictOperators.UnderlineThickness, out var xs) ? xs[0] : 50; }
    public IntOrDouble PaintType { get => Dict.TryGetValue(TopDictOperators.PaintType, out var xs) ? xs[0] : 0; }
    public IntOrDouble CharstringType { get => Dict.TryGetValue(TopDictOperators.CharstringType, out var xs) ? xs[0] : 2; }
    public IntOrDouble[] FontMatrix { get => Dict.TryGetValue(TopDictOperators.FontMatrix, out var xs) ? xs : [0.001, 0.0, 0.0, 0.001, 0.0, 0.0]; }
    public IntOrDouble? UniqueID { get => Dict.TryGetValue(TopDictOperators.UniqueID, out var xs) ? xs[0] : null; }
    public IntOrDouble[] FontBBox { get => Dict.TryGetValue(TopDictOperators.FontBBox, out var xs) ? xs : [0, 0, 0, 0]; }
    public IntOrDouble StrokeWidth { get => Dict.TryGetValue(TopDictOperators.StrokeWidth, out var xs) ? xs[0] : 0; }
    public IntOrDouble[]? XUID { get => Dict.TryGetValue(TopDictOperators.XUID, out var xs) ? xs : null; }
    public int CharsetOffset { get => Dict.TryGetValue(TopDictOperators.Charset, out var xs) ? xs[0].ToInt() : 0; }
    public int EncodingOffset { get => Dict.TryGetValue(TopDictOperators.Encoding, out var xs) ? xs[0].ToInt() : 0; }
    public int? CharStringsOffset { get => Dict.TryGetValue(TopDictOperators.CharStrings, out var xs) ? xs[0].ToInt() : null; }
    public (int Size, int Offset)? PrivateOffset { get => Dict.TryGetValue(TopDictOperators.Private, out var xs) ? (xs[0].ToInt(), xs[1].ToInt()) : null; }
    public int? SyntheticBase { get => Dict.TryGetValue(TopDictOperators.SyntheticBase, out var xs) ? xs[0].ToInt() : null; }
    public string PostScript { get => Dict.TryGetValue(TopDictOperators.PostScript, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontName { get => Dict.TryGetValue(TopDictOperators.BaseFontName, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontBlend { get => Dict.TryGetValue(TopDictOperators.BaseFontBlend, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsCIDFont { get => Dict.ContainsKey(TopDictOperators.ROS); }
    public (string Registry, string Ordering, int Supplement)? ROS { get => Dict.TryGetValue(TopDictOperators.ROS, out var xs) ? (SID.SIDToString(Strings, xs[0].ToInt()), SID.SIDToString(Strings, xs[1].ToInt()), xs[2].ToInt()) : null; }
    public IntOrDouble CIDFontVersion { get => Dict.TryGetValue(TopDictOperators.CIDFontVersion, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontRevision { get => Dict.TryGetValue(TopDictOperators.CIDFontRevision, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontType { get => Dict.TryGetValue(TopDictOperators.CIDFontType, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDCount { get => Dict.TryGetValue(TopDictOperators.CIDCount, out var xs) ? xs[0] : 8720; }
    public IntOrDouble? UIDBase { get => Dict.TryGetValue(TopDictOperators.UIDBase, out var xs) ? xs[0] : null; }
    public int? FDArrayOffset { get => Dict.TryGetValue(TopDictOperators.FDArray, out var xs) ? xs[0].ToInt() : null; }
    public int? FDSelectOffset { get => Dict.TryGetValue(TopDictOperators.FDSelect, out var xs) ? xs[0].ToInt() : null; }
    public string FontName { get => Dict.TryGetValue(TopDictOperators.FontName, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }

    public static TopDict ReadFrom(byte[] bytes, string[] strings, Stream stream, long offset)
    {
        var dict = new TopDict() { Dict = BytesToDict<TopDictOperators>(bytes) };

        var char_strings = dict.CharStringsOffset is { } char_strings_offset ? CompactFontFormat.ReadIndexData(stream.SeekTo(offset + char_strings_offset)) : [];
        var charsets = dict.Dict.ContainsKey(TopDictOperators.Charset)
            ? (dict.CharsetOffset is 0 or 1 or 2
                ? ReadDefinedCharsets(dict.CharsetOffset)
                : ReadCharsets(stream.SeekTo(offset + dict.CharsetOffset), char_strings.Length - 1))
            : null;
        var private_dict = dict.PrivateOffset is { } private_size_offset ? PrivateDict.ReadFrom(stream.SeekTo(offset + private_size_offset.Offset).ReadExactly(private_size_offset.Size), stream, offset + private_size_offset.Offset) : null;

        TopDict[]? fdarray = null;
        byte[]? fdselect = null;
        if (dict.IsCIDFont)
        {
            fdarray = [.. CompactFontFormat.ReadIndexData(stream.SeekTo(offset + dict.FDArrayOffset.Try())).Select(x => ReadFrom(x, strings, stream, offset))];
            fdselect = ReadFDSelect(stream.SeekTo(offset + dict.FDSelectOffset.Try()), char_strings.Length);
        }

        return new()
        {
            Dict = dict.Dict,
            Strings = strings,
            CharStrings = char_strings,
            Charsets = charsets,
            PrivateDict = private_dict,
            FontDictArray = fdarray ?? [],
            FontDictSelect = fdselect ?? [],
        };
    }

    public static Dictionary<T, IntOrDouble[]> BytesToDict<T>(byte[] bytes) where T : struct, Enum
    {
        var kv = new Dictionary<T, IntOrDouble[]>();
        var values = new List<IntOrDouble>();
        for (var i = 0; i < bytes.Length; i++)
        {
            var b0 = bytes[i];

            if (b0 is 12)
            {
                var b1 = bytes[++i];
                var k = (b0 * 100) + b1;
                Debug.Assert(Enum.IsDefined(typeof(T), (int)k));
                kv.Add((T)(object)k, [.. values]);
                values.Clear();
            }
            else if (b0 is >= 0 and <= 21)
            {
                Debug.Assert(Enum.IsDefined(typeof(T), (int)b0));
                kv.Add((T)(object)(int)b0, [.. values]);
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

    public static double PackedBCDToDouble(IEnumerable<byte> bytes) => bytes
        .Select(x => x switch
        {
            0x00 => "0",
            0x01 => "1",
            0x02 => "2",
            0x03 => "3",
            0x04 => "4",
            0x05 => "5",
            0x06 => "6",
            0x07 => "7",
            0x08 => "8",
            0x09 => "9",
            0x0a => ".",
            0x0b => "e",
            0x0c => "e-",
            0x0e => "-",
            _ => throw new(),
        })
        .Join()
        .To(double.Parse);

    public static IEnumerable<byte> DoubleToPackedBCD(double d) => d.ToString(CultureInfo.InvariantCulture)
        .Replace("E-", "e")
        .Replace("E+", "E")
        .Select(x => x switch
        {
            '0' => (byte)0x00,
            '1' => (byte)0x01,
            '2' => (byte)0x02,
            '3' => (byte)0x03,
            '4' => (byte)0x04,
            '5' => (byte)0x05,
            '6' => (byte)0x06,
            '7' => (byte)0x07,
            '8' => (byte)0x08,
            '9' => (byte)0x09,
            '.' => (byte)0x0a,
            'E' => (byte)0x0b, // E is e+
            'e' => (byte)0x0c, // e is e-
            '-' => (byte)0x0e,
            _ => throw new(),
        });

    public static Charsets ReadDefinedCharsets(int charset) => throw new();

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
                Glyph = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(glyph_count_without_notdef)],
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
            return [.. Lists.Repeat(stream.ReadUByte).Take(glyph_count_with_notdef)];
        }
    }

    public void WriteWithoutDictAndOffsetUpdate(Stream stream, long offset)
    {
        if (Dict.ContainsKey(TopDictOperators.Charset))
        {
            Dict[TopDictOperators.Charset] = [stream.Position - offset];
            WriteCharsets(stream, Charsets!);
        }
        if (Dict.ContainsKey(TopDictOperators.Encoding))
        {
            Debug.Fail("Encoding not support");
        }
        if (Dict.ContainsKey(TopDictOperators.CharStrings))
        {
            Dict[TopDictOperators.CharStrings] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, CharStrings);
        }
        if (Dict.ContainsKey(TopDictOperators.Private))
        {
            var private_position = stream.Position;
            var private_data = DictDataToBytes(PrivateDict!.Dict);
            Dict[TopDictOperators.Private] = [private_data.Length, private_position - offset];

            PrivateDict!.WriteWithoutDictAndOffsetUpdate(stream.SeekTo(private_data.Length, SeekOrigin.Current), private_position);

            var lastposition = stream.Position;
            stream.SeekTo(private_position).Write(DictDataToBytes(PrivateDict!.Dict));
            stream.Position = lastposition;
        }
        if (IsCIDFont)
        {
            Debug.Assert(Dict.ContainsKey(TopDictOperators.FDArray));
            Debug.Assert(Dict.ContainsKey(TopDictOperators.FDSelect));

            FontDictArray.Each(x => x.WriteWithoutDictAndOffsetUpdate(stream, offset));
            Dict[TopDictOperators.FDArray] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, [.. FontDictArray.Select(x => DictDataToBytes(x.Dict))]);

            Dict[TopDictOperators.FDSelect] = [stream.Position - offset];
            WriteFDSelect(stream, FontDictSelect);
        }
    }

    public static byte[] DictDataToBytes<T>(Dictionary<T, IntOrDouble[]> kv) where T : struct, Enum
    {
        var xs = new List<byte>();
        foreach (var (key, vs) in kv)
        {
            var k = (int)(object)key;
            for (var i = 0; i < vs.Length; i++)
            {
                if (k is 15 or 16 or 17 or 1236 or 1237 or 19 || (k == 18 && i == 1))
                {
                    // offset will be changed later, so output fixed 5 bytes.
                    xs.AddRange(DictDataNumberTo5Bytes(vs[i].ToInt()));
                }
                else if (vs[i].IsInt())
                {
                    xs.AddRange(DictDataNumberToBytes(vs[i].ToInt()));
                }
                else
                {
                    xs.Add(30);
                    xs.AddRange(DoubleToPackedBCD(vs[i].ToDouble())
                            .Concat((byte)0x0f)
                            .Concat((byte)0x0f)
                            .Chunk(2)
                            .Where(x => x.Length == 2)
                            .Select(x => (byte)((x[0] << 4) + x[1]))
                            .ToArray()
                        );
                }
            }
            xs.AddRange(k >= 100 ? [(byte)(k / 100), (byte)(k % 100)] : [(byte)k]);
        }
        return [.. xs];
    }

    public static byte[] DictDataNumberToBytes(int number) =>
        number is >= -107 and <= 107 ? ([(byte)(number + 139)])
        : number is >= 108 and <= 1131 ? ([(byte)((((number - 108) >> 8) & 0xFF) + 247), (byte)(number - 108)])
        : number is >= -1131 and <= -108 ? ([(byte)(((-(number + 108) >> 8) & 0xFF) + 251), (byte)-(number + 108)])
        : number is >= -32768 and <= 32767 ? ([28, (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)])
        : DictDataNumberTo5Bytes(number);

    public static byte[] DictDataNumberTo5Bytes(int number) => ([29, (byte)((number >> 24) & 0xFF), (byte)((number >> 16) & 0xFF), (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)]);

    public static void WriteCharsets(Stream stream, Charsets charsets)
    {
        stream.WriteByte(0);
        charsets.Glyph.Each(stream.WriteUShortByBigEndian);
    }

    public static void WriteFDSelect(Stream stream, byte[] fdselect)
    {
        stream.WriteByte(0);
        stream.Write(fdselect);
    }
}

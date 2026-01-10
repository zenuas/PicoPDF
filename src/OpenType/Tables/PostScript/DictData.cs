using Mina.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class DictData
{
    public string[] Strings { get; init; } = [];
    public required Dictionary<int, IntOrDouble[]> Dict { get; init; }
    public byte[][] CharStrings { get; init; } = [];
    public Charsets? Charsets { get; init; }
    public DictData? PrivateDict { get; init; }
    public byte[][] LocalSubroutines { get; init; } = [];
    public DictData[] FontDictArray { get; init; } = [];
    public byte[] FontDictSelect { get; init; } = [];

    public string Version { get => Dict.TryGetValue(0, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Notice { get => Dict.TryGetValue(1, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Copyright { get => Dict.TryGetValue(1200, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FullName { get => Dict.TryGetValue(2, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FamilyName { get => Dict.TryGetValue(3, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Weight { get => Dict.TryGetValue(4, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsFixedPitch { get => Dict.TryGetValue(1201, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble ItalicAngle { get => Dict.TryGetValue(1202, out var xs) ? xs[0] : 0; }
    public IntOrDouble UnderlinePosition { get => Dict.TryGetValue(1203, out var xs) ? xs[0] : -100; }
    public IntOrDouble UnderlineThickness { get => Dict.TryGetValue(1204, out var xs) ? xs[0] : 50; }
    public IntOrDouble PaintType { get => Dict.TryGetValue(1205, out var xs) ? xs[0] : 0; }
    public IntOrDouble CharstringType { get => Dict.TryGetValue(1206, out var xs) ? xs[0] : 2; }
    public IntOrDouble[] FontMatrix { get => Dict.TryGetValue(1207, out var xs) ? xs : [0.001, 0.0, 0.0, 0.001, 0.0, 0.0]; }
    public IntOrDouble? UniqueID { get => Dict.TryGetValue(13, out var xs) ? xs[0] : null; }
    public IntOrDouble[] FontBBox { get => Dict.TryGetValue(5, out var xs) ? xs : [0, 0, 0, 0]; }
    public IntOrDouble StrokeWidth { get => Dict.TryGetValue(1208, out var xs) ? xs[0] : 0; }
    public IntOrDouble[]? XUID { get => Dict.TryGetValue(14, out var xs) ? xs : null; }
    public int CharsetOffset { get => Dict.TryGetValue(15, out var xs) ? xs[0].ToInt() : 0; }
    public int EncodingOffset { get => Dict.TryGetValue(16, out var xs) ? xs[0].ToInt() : 0; }
    public int? CharStringsOffset { get => Dict.TryGetValue(17, out var xs) ? xs[0].ToInt() : null; }
    public (int Size, int Offset)? PrivateOffset { get => Dict.TryGetValue(18, out var xs) ? (xs[0].ToInt(), xs[1].ToInt()) : null; }
    public int? SyntheticBase { get => Dict.TryGetValue(1220, out var xs) ? xs[0].ToInt() : null; }
    public string PostScript { get => Dict.TryGetValue(1221, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontName { get => Dict.TryGetValue(1222, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontBlend { get => Dict.TryGetValue(1223, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsCIDFont { get => Dict.ContainsKey(1230); }
    public (string Registry, string Ordering, int Supplement)? ROS { get => Dict.TryGetValue(1230, out var xs) ? (SID.SIDToString(Strings, xs[0].ToInt()), SID.SIDToString(Strings, xs[1].ToInt()), xs[2].ToInt()) : null; }
    public IntOrDouble CIDFontVersion { get => Dict.TryGetValue(1231, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontRevision { get => Dict.TryGetValue(1232, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontType { get => Dict.TryGetValue(1233, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDCount { get => Dict.TryGetValue(1234, out var xs) ? xs[0] : 8720; }
    public IntOrDouble? UIDBase { get => Dict.TryGetValue(1235, out var xs) ? xs[0] : null; }
    public int? FDArrayOffset { get => Dict.TryGetValue(1236, out var xs) ? xs[0].ToInt() : null; }
    public int? FDSelectOffset { get => Dict.TryGetValue(1237, out var xs) ? xs[0].ToInt() : null; }
    public string FontName { get => Dict.TryGetValue(1238, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public IntOrDouble[]? BlueValues { get => Dict.TryGetValue(6, out var xs) ? xs : null; }
    public IntOrDouble[]? OtherBlues { get => Dict.TryGetValue(7, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyBlues { get => Dict.TryGetValue(8, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyOtherBlues { get => Dict.TryGetValue(9, out var xs) ? xs : null; }
    public IntOrDouble BlueScale { get => Dict.TryGetValue(1209, out var xs) ? xs[0] : 0.039625; }
    public IntOrDouble BlueShift { get => Dict.TryGetValue(1210, out var xs) ? xs[0] : 7; }
    public IntOrDouble BlueFuzz { get => Dict.TryGetValue(1211, out var xs) ? xs[0] : 1; }
    public IntOrDouble? StdHW { get => Dict.TryGetValue(10, out var xs) ? xs[0] : null; }
    public IntOrDouble? StdVW { get => Dict.TryGetValue(11, out var xs) ? xs[0] : null; }
    public IntOrDouble[]? StemSnapH { get => Dict.TryGetValue(1212, out var xs) ? xs : null; }
    public IntOrDouble[]? StemSnapV { get => Dict.TryGetValue(1213, out var xs) ? xs : null; }
    public bool ForceBold { get => Dict.TryGetValue(1214, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble LanguageGroup { get => Dict.TryGetValue(1211, out var xs) ? xs[0] : 0; }
    public IntOrDouble ExpansionFactor { get => Dict.TryGetValue(1211, out var xs) ? xs[0] : 0.06; }
    public IntOrDouble InitialRandomSeed { get => Dict.TryGetValue(1211, out var xs) ? xs[0] : 0; }
    public int? SubrsOffset { get => Dict.TryGetValue(19, out var xs) ? xs[0].ToInt() : null; }
    public int DefaultWidthX { get => Dict.TryGetValue(20, out var xs) ? xs[0].ToInt() : 0; }
    public int NominalWidthX { get => Dict.TryGetValue(21, out var xs) ? xs[0].ToInt() : 0; }


    public static DictData ReadFrom(byte[] bytes, string[] strings, Stream stream, long offset)
    {
        var dict = new DictData() { Dict = BytesToDict(bytes) };

        var char_strings = dict.CharStringsOffset is { } char_strings_offset ? CompactFontFormat.ReadIndexData(stream.SeekTo(offset + char_strings_offset)) : [];
        var charsets = dict.Dict.ContainsKey(15)
            ? (dict.CharsetOffset is 0 or 1 or 2
                ? ReadDefinedCharsets(dict.CharsetOffset)
                : ReadCharsets(stream.SeekTo(offset + dict.CharsetOffset), char_strings.Length - 1))
            : null;
        var private_dict = dict.PrivateOffset is { } private_size_offset ? ReadFrom(stream.SeekTo(offset + private_size_offset.Offset).ReadExactly(private_size_offset.Size), strings, stream, offset + private_size_offset.Offset) : null;
        var subr = dict.SubrsOffset is { } subr_offset ? CompactFontFormat.ReadIndexData(stream.SeekTo(offset + subr_offset)) : [];

        DictData[]? fdarray = null;
        byte[]? fdselect = null;
        if (dict.IsCIDFont)
        {
            fdarray = [.. CompactFontFormat.ReadIndexData(stream.SeekTo(offset + dict.FDArrayOffset.Try())).Select(x => ReadFrom(x, strings, stream, offset))];
            fdselect = ReadFDSelect(stream.SeekTo(offset + dict.FDSelectOffset.Try()), char_strings.Length);
        }

        return new()
        {
            Strings = strings,
            Dict = dict.Dict,
            CharStrings = char_strings,
            Charsets = charsets,
            PrivateDict = private_dict,
            LocalSubroutines = subr,
            FontDictArray = fdarray ?? [],
            FontDictSelect = fdselect ?? [],
        };
    }

    public DictData Clone() => new()
    {
        Strings = Strings,
        Dict = Dict.ToDictionary(),
        CharStrings = CharStrings,
        Charsets = Charsets,
        PrivateDict = PrivateDict?.Clone(),
        LocalSubroutines = LocalSubroutines,
        FontDictArray = [.. FontDictArray.Select(x => x.Clone())],
        FontDictSelect = [.. FontDictSelect],
    };

    public static Dictionary<int, IntOrDouble[]> BytesToDict(byte[] bytes)
    {
        var kv = new Dictionary<int, IntOrDouble[]>();
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
        .To(x => double.Parse(x));

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
                Glyph = [.. Enumerable.Repeat(0, glyph_count_without_notdef).Select(_ => stream.ReadUShortByBigEndian())],
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
            return [.. Enumerable.Repeat(0, glyph_count_with_notdef).Select(_ => stream.ReadUByte())];
        }
    }

    public void WriteWithoutDictAndOffsetUpdate(Stream stream, long offset)
    {
        if (Dict.ContainsKey(15))
        {
            Dict[15] = [stream.Position - offset];
            WriteCharsets(stream, Charsets!);
        }
        if (Dict.ContainsKey(16))
        {
            Debug.Fail("Encoding not support");
        }
        if (Dict.ContainsKey(17))
        {
            Dict[17] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, CharStrings);
        }
        if (Dict.ContainsKey(18))
        {
            var private_position = stream.Position;
            var private_data = DictDataToBytes(PrivateDict!.Dict);
            Dict[18] = [private_data.Length, private_position - offset];

            PrivateDict!.WriteWithoutDictAndOffsetUpdate(stream.SeekTo(private_data.Length, SeekOrigin.Current), private_position);

            var lastposition = stream.Position;
            stream.SeekTo(private_position).Write(DictDataToBytes(PrivateDict!.Dict));
            stream.Position = lastposition;
        }
        if (IsCIDFont)
        {
            Debug.Assert(Dict.ContainsKey(1236));
            Debug.Assert(Dict.ContainsKey(1237));

            FontDictArray.Each(x => x.WriteWithoutDictAndOffsetUpdate(stream, offset));
            Dict[1236] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, [.. FontDictArray.Select(x => DictDataToBytes(x.Dict))]);

            Dict[1237] = [stream.Position - offset];
            WriteFDSelect(stream, FontDictSelect);
        }
        if (Dict.ContainsKey(19))
        {
            Dict[19] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, LocalSubroutines);
        }
    }

    public static byte[] DictDataToBytes(Dictionary<int, IntOrDouble[]> kv)
    {
        var xs = new List<byte>();
        foreach (var (k, vs) in kv)
        {
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
        fdselect.Each(stream.WriteByte);
    }
}

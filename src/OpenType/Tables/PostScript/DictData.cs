using Mina.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class DictData : Dictionary<int, IntOrDouble[]>
{
    public string[] Strings { get; set; } = [];

    public string Version { get => TryGetValue(0, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Notice { get => TryGetValue(1, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Copyright { get => TryGetValue(1200, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FullName { get => TryGetValue(2, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string FamilyName { get => TryGetValue(3, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string Weight { get => TryGetValue(4, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsFixedPitch { get => TryGetValue(1201, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble ItalicAngle { get => TryGetValue(1202, out var xs) ? xs[0] : 0; }
    public IntOrDouble UnderlinePosition { get => TryGetValue(1203, out var xs) ? xs[0] : -100; }
    public IntOrDouble UnderlineThickness { get => TryGetValue(1204, out var xs) ? xs[0] : 50; }
    public IntOrDouble PaintType { get => TryGetValue(1205, out var xs) ? xs[0] : 0; }
    public IntOrDouble CharstringType { get => TryGetValue(1206, out var xs) ? xs[0] : 2; }
    public IntOrDouble[] FontMatrix { get => TryGetValue(1207, out var xs) ? xs : [0.001, 0.0, 0.0, 0.001, 0.0, 0.0]; }
    public IntOrDouble? UniqueID { get => TryGetValue(13, out var xs) ? xs[0] : null; }
    public IntOrDouble[] FontBBox { get => TryGetValue(5, out var xs) ? xs : [0, 0, 0, 0]; }
    public IntOrDouble StrokeWidth { get => TryGetValue(1208, out var xs) ? xs[0] : 0; }
    public IntOrDouble[]? XUID { get => TryGetValue(14, out var xs) ? xs : null; }
    public int Charset { get => TryGetValue(15, out var xs) ? xs[0].ToInt() : 0; }
    public int Encoding { get => TryGetValue(16, out var xs) ? xs[0].ToInt() : 0; }
    public int? CharStrings { get => TryGetValue(17, out var xs) ? xs[0].ToInt() : null; }
    public (int Size, int Offset)? Private { get => TryGetValue(18, out var xs) ? (xs[0].ToInt(), xs[1].ToInt()) : null; }
    public int? SyntheticBase { get => TryGetValue(1220, out var xs) ? xs[0].ToInt() : null; }
    public string PostScript { get => TryGetValue(1221, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontName { get => TryGetValue(1222, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public string BaseFontBlend { get => TryGetValue(1223, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public bool IsCIDFont { get => ContainsKey(1230); }
    public (string Registry, string Ordering, int Supplement)? ROS { get => TryGetValue(1230, out var xs) ? (SID.SIDToString(Strings, xs[0].ToInt()), SID.SIDToString(Strings, xs[1].ToInt()), xs[2].ToInt()) : null; }
    public IntOrDouble CIDFontVersion { get => TryGetValue(1231, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontRevision { get => TryGetValue(1232, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDFontType { get => TryGetValue(1233, out var xs) ? xs[0] : 0; }
    public IntOrDouble CIDCount { get => TryGetValue(1234, out var xs) ? xs[0] : 8720; }
    public IntOrDouble? UIDBase { get => TryGetValue(1235, out var xs) ? xs[0] : null; }
    public int? FDArray { get => TryGetValue(1236, out var xs) ? xs[0].ToInt() : null; }
    public int? FDSelect { get => TryGetValue(1237, out var xs) ? xs[0].ToInt() : null; }
    public string FontName { get => TryGetValue(1238, out var xs) ? SID.SIDToString(Strings, xs[0].ToInt()) : ""; }
    public IntOrDouble[]? BlueValues { get => TryGetValue(6, out var xs) ? xs : null; }
    public IntOrDouble[]? OtherBlues { get => TryGetValue(7, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyBlues { get => TryGetValue(8, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyOtherBlues { get => TryGetValue(9, out var xs) ? xs : null; }
    public IntOrDouble BlueScale { get => TryGetValue(1209, out var xs) ? xs[0] : 0.039625; }
    public IntOrDouble BlueShift { get => TryGetValue(1210, out var xs) ? xs[0] : 7; }
    public IntOrDouble BlueFuzz { get => TryGetValue(1211, out var xs) ? xs[0] : 1; }
    public IntOrDouble? StdHW { get => TryGetValue(10, out var xs) ? xs[0] : null; }
    public IntOrDouble? StdVW { get => TryGetValue(11, out var xs) ? xs[0] : null; }
    public IntOrDouble[]? StemSnapH { get => TryGetValue(1212, out var xs) ? xs : null; }
    public IntOrDouble[]? StemSnapV { get => TryGetValue(1213, out var xs) ? xs : null; }
    public bool ForceBold { get => TryGetValue(1214, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble LanguageGroup { get => TryGetValue(1211, out var xs) ? xs[0] : 0; }
    public IntOrDouble ExpansionFactor { get => TryGetValue(1211, out var xs) ? xs[0] : 0.06; }
    public IntOrDouble InitialRandomSeed { get => TryGetValue(1211, out var xs) ? xs[0] : 0; }
    public int? Subrs { get => TryGetValue(19, out var xs) ? xs[0].ToInt() : null; }
    public int DefaultWidthX { get => TryGetValue(20, out var xs) ? xs[0].ToInt() : 0; }
    public int NominalWidthX { get => TryGetValue(21, out var xs) ? xs[0].ToInt() : 0; }


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

    public static double PackedBCDToDouble(byte[] bytes) => bytes
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

    public static byte[] DictDataToBytes(Dictionary<int, IntOrDouble[]> kv)
    {
        using var mem = new MemoryStream();
        foreach (var (k, vs) in kv)
        {
            vs.Each(x => mem.Write(DictDataNumberToBytes(x.ToInt())));
            mem.Write(k >= 100 ? [(byte)(k / 100), (byte)(k % 100)] : [(byte)k]);
        }
        return mem.ToArray();
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

    public static byte[] DictDataNumberToBytes(int number) =>
        number is >= -107 and <= 107 ? ([(byte)(number + 139)])
        : number is >= 108 and <= 1131 ? ([(byte)((((number - 108) >> 8) & 0xFF) + 247), (byte)(number - 108)])
        : number is >= -1131 and <= -108 ? ([(byte)(((-(number + 108) >> 8) & 0xFF) + 251), (byte)-(number + 108)])
        : number is >= -32768 and <= 32767 ? ([28, (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)])
        : DictDataNumberTo5Bytes(number);

    public static byte[] DictDataNumberTo5Bytes(int number) => ([29, (byte)((number >> 24) & 0xFF), (byte)((number >> 16) & 0xFF), (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)]);

}

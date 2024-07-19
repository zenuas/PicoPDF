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
    public required DictData TopDict { get; init; }
    public required string[] Strings { get; init; }
    public required byte[][] GlobalSubroutines { get; init; }
    public required byte[][] CharStrings { get; init; }
    public required Charsets Charsets { get; init; }
    public required DictData PrivateDict { get; init; }
    public required (IntOrDouble FontName, DictData Private, byte[][] LocalSubroutines)[] FontDictArray { get; init; }
    public required byte[] FontDictSelect { get; init; }

    public static CompactFontFormat ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major = stream.ReadUByte();
        var minor = stream.ReadUByte();
        var header_size = stream.ReadUByte();
        var offset_size = stream.ReadUByte();
        var names = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var top_dict = ReadIndexData(stream).Select(DictData.ReadFrom).First();
        var strings = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var global_subr = ReadIndexData(stream);

        var char_strings = top_dict.CharStrings is { } char_strings_offset ? ReadIndexData(stream, position + char_strings_offset) : [];

        stream.Position = position + top_dict.Charset;
        var charsets = ReadCharsets(stream, char_strings.Length - 1);

        var private_dict = top_dict.Private is { } private_size_offset ? DictData.ReadFrom(stream.ReadPositionBytes(position + private_size_offset.Offset, private_size_offset.Size)) : [];

        (IntOrDouble, DictData, byte[][])[]? fdarray = null;
        byte[]? fdselect = null;
        if (top_dict.IsCIDFont)
        {
            var fdarray_offset = top_dict.FDArray is { } xfdarray ? xfdarray : throw new();
            var fdselect_offset = top_dict.FDSelect is { } xfdselect ? xfdselect : throw new();

            fdarray = ReadIndexData(stream, position + fdarray_offset).Select(x =>
                {
                    var dict = DictData.ReadFrom(x);
                    var (private_size, private_offset) = dict.Private is { } xprivate ? xprivate : throw new();
                    var fd_private_offset = position + private_offset;
                    var fd_private_dict = DictData.ReadFrom(stream.ReadPositionBytes(fd_private_offset, private_size));
                    var subr = fd_private_dict.Subrs is { } subr_offset ? ReadIndexData(stream, fd_private_offset + subr_offset) : [];
                    return (dict[1238][0], fd_private_dict, subr);
                }).ToArray();

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

    public static byte[][] ReadIndexData(Stream stream, long position)
    {
        stream.Position = position;
        return ReadIndexData(stream);
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
        WriteIndexData(stream, [DictData.DictDataTo5Bytes(top_dict)]);
        WriteIndexData(stream, Strings.Select(Encoding.UTF8.GetBytes).ToArray());
        WriteIndexData(stream, GlobalSubroutines);

        top_dict[15] = [stream.Position - position];
        WriteCharsets(stream, Charsets);

        top_dict[17] = [stream.Position - position];
        WriteIndexData(stream, CharStrings);

        if (PrivateDict.Count > 0)
        {
            var private_dict = DictData.DictDataToBytes(PrivateDict);
            top_dict[18] = [private_dict.Length, stream.Position - position];
            stream.Write(private_dict);
        }

        if (top_dict.ContainsKey(1230))
        {
            var fdarray = new List<byte[]>();
            var fdarray_dict = new Dictionary<int, IntOrDouble[]>();
            foreach (var (fontname, fd_private_dict, subr) in FontDictArray)
            {
                var fd_private_offset = stream.Position;
                var fd_private_data = DictData.DictDataTo5Bytes(fd_private_dict);
                fdarray_dict[1238] = [fontname];
                fdarray_dict[18] = [fd_private_data.Length, fd_private_offset - position];
                stream.Write(fd_private_data);
                if (subr.Length > 0)
                {
                    fd_private_dict[19] = [stream.Position - fd_private_offset];
                    WriteIndexData(stream, subr);
                    var subr_lastposition = stream.Position;
                    stream.Position = fd_private_offset;
                    stream.Write(DictData.DictDataTo5Bytes(fd_private_dict));
                    stream.Position = subr_lastposition;
                }
                fdarray.Add(DictData.DictDataToBytes(fdarray_dict));
                fdarray_dict.Clear();
            }
            top_dict[1236] = [stream.Position - position];
            WriteIndexData(stream, [.. fdarray]);

            top_dict[1237] = [stream.Position - position];
            WriteFDSelect(stream, FontDictSelect);
        }

        var lastposition = stream.Position;
        stream.Position = top_dict_start;
        WriteIndexData(stream, [DictData.DictDataTo5Bytes(top_dict)]);
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

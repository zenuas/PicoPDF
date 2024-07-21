using Mina.Extension;
using System;
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
    public required DictData? PrivateDict { get; init; }
    public required DictData[] FontDictArray { get; init; }
    public required byte[] FontDictSelect { get; init; }

    public static CompactFontFormat ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major = stream.ReadUByte();
        var minor = stream.ReadUByte();
        var header_size = stream.ReadUByte();
        var offset_size = stream.ReadUByte();
        var names = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var top_dict_data = ReadIndexData(stream).First();
        var strings = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var global_subr = ReadIndexData(stream);
        var top_dict = DictData.ReadFrom(top_dict_data, strings, stream, position);

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
            CharStrings = top_dict.CharStrings,
            Charsets = top_dict.Charsets.Try(),
            PrivateDict = top_dict.PrivateDict,
            FontDictArray = top_dict.FontDictArray,
            FontDictSelect = top_dict.FontDictSelect,
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

    public void WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteByte(Major);
        stream.WriteByte(Minor);
        stream.WriteByte(HeaderSize);
        stream.WriteByte(OffsetSize);
        WriteIndexData(stream, Names.Select(Encoding.UTF8.GetBytes).ToArray());

        var top_dict_start = stream.Position;
        WriteIndexData(stream, [DictData.DictDataToBytes(TopDict.Dict)]);
        WriteIndexData(stream, Strings.Select(Encoding.UTF8.GetBytes).ToArray());
        WriteIndexData(stream, GlobalSubroutines);

        TopDict.WriteToDataAndOffsetUpdate(stream, position, CharStrings, Charsets, PrivateDict, TopDict.LocalSubroutines, FontDictArray, FontDictSelect);

        var lastposition = stream.Position;
        stream.Position = top_dict_start;
        WriteIndexData(stream, [DictData.DictDataToBytes(TopDict.Dict)]);
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

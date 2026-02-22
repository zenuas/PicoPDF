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
        };
    }

    public static byte[][] ReadIndexData(Stream stream) => ReadIndexDataBody(stream, ReadIndexDataHeader(stream));

    public static int[] ReadIndexDataHeader(Stream stream)
    {
        var count = stream.ReadUShortByBigEndian();
        if (count == 0) return [0];
        var offset_size = stream.ReadUByte();

        Func<Stream, int> offset_read = offset_size switch
        {
            1 => (x) => x.ReadUByte(),
            2 => (x) => x.ReadUShortByBigEndian(),
            3 => (x) => (x.ReadUByte() << 16) | (x.ReadUByte() << 8) | x.ReadUByte(),
            _ => (x) => (int)x.ReadUIntByBigEndian(),
        };

        return [.. Lists.Repeat(() => offset_read(stream)).Take(count + 1)];
    }

    public static byte[][] ReadIndexDataBody(Stream stream, int[] offset) => [.. Enumerable.Range(0, offset.Length - 1).Select(i => stream.ReadExactly(offset[i + 1] - offset[i]))];

    public void WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteByte(Major);
        stream.WriteByte(Minor);
        stream.WriteByte(HeaderSize);
        stream.WriteByte(OffsetSize);
        WriteIndexData(stream, [.. Names.Select(Encoding.UTF8.GetBytes)]);

        var top_dict_start = stream.Position;
        WriteIndexData(stream, [DictData.DictDataToBytes(TopDict.Dict)]);
        WriteIndexData(stream, [.. Strings.Select(Encoding.UTF8.GetBytes)]);
        WriteIndexData(stream, GlobalSubroutines);

        TopDict.WriteWithoutDictAndOffsetUpdate(stream, position);

        var lastposition = stream.Position;
        WriteIndexData(stream.SeekTo(top_dict_start), [DictData.DictDataToBytes(TopDict.Dict)]);
        stream.Position = lastposition;
    }

    public static void WriteIndexData(Stream stream, byte[][] index)
    {
        WriteIndexDataHeader(stream, [.. index.Select(x => (uint)x.Length)]);
        WriteIndexDataBody(stream, index);
    }

    public static void WriteIndexDataHeader(Stream stream, uint[] index)
    {
        stream.WriteUShortByBigEndian((ushort)index.Length);
        if (index.Length == 0) return;

        var offset_max = index.Sum(x => x) + 1;
        var offset_size =
            offset_max <= byte.MaxValue ? 1 :
            offset_max <= ushort.MaxValue ? 2 :
            offset_max <= 0xFFFFFF ? 3 :
            4;
        stream.WriteByte((byte)offset_size);

        Action<Stream, uint> offset_write = offset_size switch
        {
            1 => (x, n) => x.WriteByte((byte)n),
            2 => (x, n) => x.WriteUShortByBigEndian((ushort)n),
            3 => (x, n) => x.Write([(byte)((n >> 16) & 0xFF), (byte)((n >> 8) & 0xFF), (byte)(n & 0xFF)]),
            _ => (x, n) => x.WriteUIntByBigEndian(n),
        };

        var offset = 1U;
        offset_write(stream, 1);
        index.Each(x => offset_write(stream, offset += x));
    }

    public static void WriteIndexDataBody(Stream stream, byte[][] index) => index.Each(x => stream.Write(x));
}

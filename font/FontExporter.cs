using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenType;

public static class FontExporter
{
    public static byte[] Export(IOpenTypeFont font)
    {
        using var stream = new MemoryStream();
        Export(font, stream);
        return stream.ToArray();
    }

    public static void Export(IOpenTypeFont font, Stream stream, long start_stream_position = 0)
    {
        var table_names = new string[] { "cmap", "head", "hhea", "hmtx", "maxp", "name", "post" }
            .If(_ => font is PostScriptFont, xs => xs.Concat("CFF "), xs => xs)
            .If(_ => font.Color is { }, xs => xs.Concat("COLR"), xs => xs)
            .If(_ => font.ColorPalette is { }, xs => xs.Concat("CPAL"), xs => xs)
            .If(_ => font.OS2 is { }, xs => xs.Concat("OS/2"), xs => xs)
            .If(_ => font is TrueTypeFont, xs => xs.Concat(["glyf", "loca"]), xs => xs)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var tables = table_names.ToDictionary(x => x, _ => new MutableTableRecord { Position = 0, Checksum = 0, Offset = 0, Length = 0 });

        var tables_pow = (int)Math.Pow(2, Math.Floor(Math.Log2(tables.Count)));
        new OffsetTable
        {
            SfntVersion = font.Offset.SfntVersion,
            NumberOfTables = (ushort)tables.Count,
            SearchRange = (ushort)(tables_pow * 16),
            EntrySelector = (ushort)Math.Log2(tables_pow),
            RangeShift = (ushort)((tables.Count * 16) - (tables_pow * 16)),
        }.WriteTo(stream);

        foreach (var x in table_names)
        {
            var table = tables[x];
            stream.WriteTag(x);
            table.Position = stream.Position;
            WriteTableRecord(stream, table);
        }

        if (font.Color is { }) ExportTable(stream, start_stream_position, tables["COLR"], font.Color);
        if (font.ColorPalette is { }) ExportTable(stream, start_stream_position, tables["CPAL"], font.ColorPalette);
        if (font.OS2 is { }) ExportTable(stream, start_stream_position, tables["OS/2"], font.OS2);
        ExportTable(stream, start_stream_position, tables["cmap"], font.CMap);
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0));
        ExportTable(stream, start_stream_position, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, start_stream_position, tables["hmtx"], font.HorizontalMetrics);
        ExportTable(stream, start_stream_position, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, start_stream_position, tables["name"], font.Name);
        ExportTable(stream, start_stream_position, tables["post"], font.PostScript);

        if (font.OS2 is { }) Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);
        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["hhea"].Length == 36);
        Debug.Assert(tables["maxp"].Length is 6 or 32);
        Debug.Assert(tables["post"].Length == 32);

        switch (font)
        {
            case TrueTypeFont ttf: Export(ttf, tables, stream, start_stream_position); break;
            case PostScriptFont psf: Export(psf, tables, stream, start_stream_position); break;
            case NoOutlineFont noo: Export(noo, tables, stream, start_stream_position); break;
            default: throw new();
        }

        var lastposition = stream.Position;
        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
        var checksum = CalcChecksum(stream.SeekTo(start_stream_position), (uint)(lastposition - start_stream_position));
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0xB1B0AFBA - checksum));
        stream.Position = lastposition;
    }

    public static void Export(TrueTypeFont font, Dictionary<string, MutableTableRecord> tables, Stream stream, long start_stream_position = 0)
    {
        // glyf table export
        var glyf_start = stream.Position;
        var glyph_index = new Dictionary<int, long>();
        tables["glyf"].Offset = (uint)(glyf_start - start_stream_position);
        var index_to_locformat = font.FontHeader.IndexToLocFormat;
        var lastglyph = font.Glyphs
            .Select((x, i) => (Glyph: x, Index: i))
            .Aggregate(0L, (acc, x) =>
            {
                glyph_index[x.Index] = acc;
                var position = stream.Position;
                x.Glyph.WriteTo(stream);
                return acc + (stream.Position - position) + (index_to_locformat == 0 ? StreamAlignment(stream, 2) : 0);
            });
        tables["glyf"].Length = (uint)(stream.Position - glyf_start);
        _ = StreamAlignment(stream);

        // loca table export
        var loca_start = stream.Position;
        tables["loca"].Offset = (uint)(loca_start - start_stream_position);
        if (index_to_locformat == 0)
        {
            font.Glyphs.Each((_, i) => stream.WriteOffset16((ushort)(glyph_index[i] / 2)));
            stream.WriteOffset16((ushort)(lastglyph / 2));
        }
        else
        {
            font.Glyphs.Each((_, i) => stream.WriteOffset32((uint)(glyph_index[i])));
            stream.WriteOffset32((uint)lastglyph);
        }
        tables["loca"].Length = (uint)(stream.Position - loca_start);
        _ = StreamAlignment(stream);
    }

    public static void Export(PostScriptFont font, Dictionary<string, MutableTableRecord> tables, Stream stream, long start_stream_position = 0)
    {
        ExportTable(stream, start_stream_position, tables["CFF "], font.CompactFontFormat);
    }

    public static void Export(NoOutlineFont font, Dictionary<string, MutableTableRecord> tables, Stream stream, long start_stream_position = 0)
    {
    }

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, IExportable table) => ExportTable(stream, start_stream_position, rec, table.WriteTo);

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, Action<Stream> f)
    {
        rec.Offset = (uint)(stream.Position - start_stream_position);
        Debug.Assert((rec.Offset % 4) == 0);
        var position = stream.Position;
        f(stream);
        rec.Length = (uint)(stream.Position - position);
        _ = StreamAlignment(stream);
    }

    public static void WriteTableRecord(Stream stream, MutableTableRecord table)
    {
        stream.WriteUIntByBigEndian(table.Checksum);
        stream.WriteOffset32(table.Offset);
        stream.WriteUIntByBigEndian(table.Length);
    }

    public static void MovePositonAndWriteTableRecord(Stream stream, MutableTableRecord table)
    {
        Debug.Assert((table.Offset % 4) == 0);
        table.Checksum = CalcChecksum(stream.SeekTo(table.Offset), table.Length);
        WriteTableRecord(stream.SeekTo(table.Position), table);
    }

    public static uint CalcChecksum(Stream stream, uint length)
    {
        var sum = 0U;
        for (var i = 0U; i < length; i += sizeof(uint))
        {
            sum += stream.ReadUIntByBigEndian();
        }
        return sum;
    }

    public static int StreamAlignment(Stream stream, int alignment = 4)
    {
        var padding = (int)(alignment - (stream.Position % alignment));
        if (padding == alignment) return 0;
        stream.Write(Lists.Repeat((byte)0).Take(padding).ToArray());
        return padding;
    }
}


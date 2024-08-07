﻿using Mina.Extension;
using PicoPDF.OpenType.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontExporter
{
    public static byte[] Export(TrueTypeFont font)
    {
        using var stream = new MemoryStream();
        Export(font, stream);
        return stream.ToArray();
    }

    public static byte[] Export(PostScriptFont font)
    {
        using var stream = new MemoryStream();
        Export(font, stream);
        return stream.ToArray();
    }

    public static void Export(TrueTypeFont font, Stream stream, long start_stream_position = 0)
    {
        var table_names = new string[] { "OS/2", "cmap", "glyf", "head", "hhea", "hmtx", "loca", "maxp", "name", "post" };
        var tables = table_names.ToDictionary(x => x, _ => new MutableTableRecord { Position = 0, Checksum = 0, Offset = 0, Length = 0 });

        var tables_pow = (int)Math.Pow(2, Math.Floor(Math.Log2(tables.Count)));
        new OffsetTable
        {
            Version = font.Offset.Version,
            NumberOfTables = (ushort)tables.Count,
            SearchRange = (ushort)(tables_pow * 16),
            EntrySelector = (ushort)Math.Log2(tables_pow),
            RangeShift = (ushort)((tables.Count * 16) - (tables_pow * 16)),
        }.WriteTo(stream);

        table_names.Each(x =>
        {
            var table = tables[x];
            stream.Write(x);
            table.Position = stream.Position;
            WriteTableRecord(stream, table);
        });

        ExportTable(stream, start_stream_position, tables["OS/2"], font.OS2);
        ExportTable(stream, start_stream_position, tables["cmap"], font.CMap);
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0));
        ExportTable(stream, start_stream_position, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, start_stream_position, tables["hmtx"], font.HorizontalMetrics);
        ExportTable(stream, start_stream_position, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, start_stream_position, tables["name"], font.Name);
        ExportTable(stream, start_stream_position, tables["post"], font.PostScript);

        Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);
        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["hhea"].Length == 36);
        Debug.Assert(tables["maxp"].Length is 6 or 32);
        Debug.Assert(tables["post"].Length == 32);

        // glyf table export
        _ = StreamAlignment(stream);
        var glyf_start = stream.Position;
        var glyph_index = new Dictionary<int, long>();
        tables["glyf"].Offset = (uint)(glyf_start - start_stream_position);
        var lastglyph = font.Glyphs
            .Select((x, i) => (Glyph: x, Index: i))
            .Aggregate(0L, (acc, x) =>
            {
                glyph_index[x.Index] = acc;
                var position = stream.Position;
                x.Glyph.WriteTo(stream);
                return acc + (stream.Position - position) + StreamAlignment(stream);
            });
        tables["glyf"].Length = (uint)(stream.Position - glyf_start);

        // loca table export
        _ = StreamAlignment(stream);
        var loca_start = stream.Position;
        tables["loca"].Offset = (uint)(loca_start - start_stream_position);
        if (font.FontHeader.IndexToLocFormat == 0)
        {
            font.Glyphs.Each((_, i) => stream.WriteUShortByBigEndian((ushort)(glyph_index[i] / 2)));
            stream.WriteUShortByBigEndian((ushort)(lastglyph / 2));
        }
        else
        {
            font.Glyphs.Each((_, i) => stream.WriteUIntByBigEndian((uint)(glyph_index[i])));
            stream.WriteUIntByBigEndian((uint)lastglyph);
        }
        tables["loca"].Length = (uint)(stream.Position - loca_start);

        var lastposition = stream.Position;
        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
        var checksum = CalcChecksum(stream.SeekTo(start_stream_position), (uint)(lastposition - start_stream_position));
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0xB1B0AFBA - checksum));
        stream.Position = lastposition;
    }

    public static void Export(PostScriptFont font, Stream stream, long start_stream_position = 0)
    {
        var table_names = new string[] { "CFF ", "OS/2", "cmap", "head", "hhea", "hmtx", "maxp", "name", "post" };
        var tables = table_names.ToDictionary(x => x, _ => new MutableTableRecord { Position = 0, Checksum = 0, Offset = 0, Length = 0 });

        var tables_pow = (int)Math.Pow(2, Math.Floor(Math.Log2(tables.Count)));
        new OffsetTable
        {
            Version = font.Offset.Version,
            NumberOfTables = (ushort)tables.Count,
            SearchRange = (ushort)(tables_pow * 16),
            EntrySelector = (ushort)Math.Log2(tables_pow),
            RangeShift = (ushort)((tables.Count * 16) - (tables_pow * 16)),
        }.WriteTo(stream);

        table_names.Each(x =>
        {
            var table = tables[x];
            stream.Write(x);
            table.Position = stream.Position;
            WriteTableRecord(stream, table);
        });

        ExportTable(stream, start_stream_position, tables["CFF "], font.CompactFontFormat);
        ExportTable(stream, start_stream_position, tables["OS/2"], font.OS2);
        ExportTable(stream, start_stream_position, tables["cmap"], font.CMap);
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0));
        ExportTable(stream, start_stream_position, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, start_stream_position, tables["hmtx"], font.HorizontalMetrics);
        ExportTable(stream, start_stream_position, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, start_stream_position, tables["name"], font.Name);
        ExportTable(stream, start_stream_position, tables["post"], font.PostScript);

        Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);
        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["hhea"].Length == 36);
        Debug.Assert(tables["maxp"].Length is 6 or 32);
        Debug.Assert(tables["post"].Length == 32);

        var lastposition = stream.Position;
        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
        var checksum = CalcChecksum(stream.SeekTo(start_stream_position), (uint)(lastposition - start_stream_position));
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0xB1B0AFBA - checksum));
        stream.Position = lastposition;
    }

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, IExportable table) => ExportTable(stream, start_stream_position, rec, table.WriteTo);

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, Action<Stream> f)
    {
        _ = StreamAlignment(stream);
        rec.Offset = (uint)(stream.Position - start_stream_position);
        var position = stream.Position;
        f(stream);
        rec.Length = (uint)(stream.Position - position);
    }

    public static void WriteTableRecord(Stream stream, MutableTableRecord table)
    {
        stream.WriteUIntByBigEndian(table.Checksum);
        stream.WriteUIntByBigEndian(table.Offset);
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


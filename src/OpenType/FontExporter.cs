using Mina.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontExporter
{
    public static void Export(TrueTypeFont font, Stream stream, long start_stream_position = 0)
    {
        var table_names = new string[] { "name", "head", "maxp", "post", "OS/2", "cmap", "hhea", "hmtx", "loca", "glyf" };
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

        ExportTable(stream, start_stream_position, tables["name"], font.Name);
        ExportTable(stream, start_stream_position, tables["head"], font.FontHeader);
        ExportTable(stream, start_stream_position, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, start_stream_position, tables["post"], font.PostScript);
        ExportTable(stream, start_stream_position, tables["OS/2"], font.OS2);
        ExportTable(stream, start_stream_position, tables["cmap"], font.CMap);
        ExportTable(stream, start_stream_position, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, start_stream_position, tables["hmtx"], font.HorizontalMetrics);

        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["maxp"].Length is 6 or 32);
        Debug.Assert(tables["post"].Length == 32);
        Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);
        Debug.Assert(tables["hhea"].Length == 36);

        // glyf table export
        var glyf_start = stream.Position;
        var glyf_index = new Dictionary<int, long>();
        tables["glyf"].Offset = (uint)(glyf_start - start_stream_position);
        var lastposition = font.Glyphs
            .Select((x, i) => (Glyph: x, Index: i))
            .Aggregate(0L, (acc, x) =>
            {
                glyf_index[x.Index] = acc;
                var position = stream.Position;
                x.Glyph.WriteTo(stream);
                return acc + (stream.Position - position);
            });
        tables["glyf"].Length = (uint)(stream.Position - glyf_start);

        // loca table export
        var loca_start = stream.Position;
        tables["loca"].Offset = (uint)(loca_start - start_stream_position);
        if (font.FontHeader.IndexToLocFormat == 0)
        {
            stream.WriteUShortByBigEndian(0);
            font.Glyphs.Each((_, i) => stream.WriteUShortByBigEndian((ushort)(glyf_index[i] / 2)));
            stream.WriteUShortByBigEndian((ushort)(lastposition / 2));
        }
        else
        {
            stream.WriteUIntByBigEndian(0);
            font.Glyphs.Each((_, i) => stream.WriteUIntByBigEndian((uint)(glyf_index[i])));
            stream.WriteUIntByBigEndian((uint)lastposition);
        }
        tables["loca"].Length = (uint)(stream.Position - loca_start);

        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
    }

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, IExportable table)
    {
        rec.Offset = (uint)(stream.Position - start_stream_position);
        var position = stream.Position;
        table.WriteTo(stream);
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
        stream.Position = table.Position;
        WriteTableRecord(stream, table);
    }
}


using Mina.Extension;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontExporter
{
    public static void Export(TrueTypeFont font, Stream stream)
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
            stream.Write(x);
            WriteTableRecord(stream, tables[x]);
        });

        ExportTable(stream, tables["name"], font.Name);
        ExportTable(stream, tables["head"], font.FontHeader);
        ExportTable(stream, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, tables["post"], font.PostScript);
        ExportTable(stream, tables["OS/2"], font.OS2);
        ExportTable(stream, tables["cmap"], font.CMap);
        ExportTable(stream, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, tables["hmtx"], font.HorizontalMetrics);
        ExportTable(stream, tables["loca"], font.IndexToLocation);

        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["post"].Length == 32);
        Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);

        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
    }

    public static void ExportTable(Stream stream, MutableTableRecord rec, IExportable table)
    {
        rec.Offset = (uint)stream.Position;
        rec.Length = (uint)table.WriteTo(stream);
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


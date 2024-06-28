using Mina.Extension;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontExporter
{
    public class MutableTableRecord { public long Position; public uint Checksum; public uint Offset; public uint Length; }

    public static void Export(TrueTypeFont font, Stream stream)
    {
        var table_names = new string[] { "name", "head" };
        var tables = table_names.ToDictionary(x => x, _ => new MutableTableRecord { Position = 0L, Checksum = 0, Offset = 0, Length = 0 });

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
            var table = tables[x];
            table.Position = stream.Position;
            WriteTableRecord(stream, table);
        });

        var head = tables["head"];
        head.Length = (uint)font.FontHeader.WriteTo(stream);
        Debug.Assert(head.Length == 54);
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


using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontLoader
{
    public static FontLoading Load(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return Load(new FontPath { Path = path }, stream, 0, opt ?? new());
    }

    public static FontLoading[] LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadCollection(path, stream, opt ?? new());
    }

    public static FontLoading[] LoadCollection(string path, Stream stream, LoadOption opt)
    {
        var header = new TrueTypeCollectionHeader(stream);
        if (header.TTCTag != "ttcf") throw new InvalidOperationException();

        return Enumerable.Range(0, (int)header.NumberOfFonts)
            .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
            .ToArray()
            .Select((x, i) => Load(new FontCollectionPath { Path = path, Index = i }, stream, x, opt))
            .ToArray();
    }

    public static FontLoading Load(IFontPath path, Stream stream, long pos, LoadOption opt)
    {
        stream.Position = pos;
        var offset = new OffsetTable(stream);
        if (!offset.ContainTrueType() && !offset.ContainCFF()) throw new InvalidOperationException();

        var tables = Enumerable.Range(0, offset.NumberOfTables)
            .Select(_ => new TableRecord(stream))
            .ToDictionary(x => x.TableTag, x => x);

        var namerecs = NameRecord.ReadFrom(stream, tables["name"]);
        var namev = (ushort nameid) => opt.PlatformIDOrder
            .Select(x => namerecs.FindFirstOrNullValue(y => y.NameRecord.PlatformID == x && y.NameRecord.NameID == nameid)?.Name)
            .Where(x => x is { })
            .FirstOrDefault();

        return new()
        {
            FontFamily = namev(1) ?? "",
            Style = namev(2) ?? "",
            FullFontName = namev(4) ?? "",
            PostScriptName = namev(6) ?? "",
            Path = path,
            Position = pos,
            TableRecords = tables,
            Offset = offset,
            NameRecords = namerecs,
        };
    }

    public static FontInfo DelayLoad(FontLoading font)
    {
        using var stream = File.OpenRead(font.Path.Path);

        var head = ReadTableRecprds(font, "head", stream, x => new FontHeaderTable(x)).Try();
        var maxp = ReadTableRecprds(font, "maxp", stream, x => new MaximumProfileTable(x)).Try();
        var post = ReadTableRecprds(font, "post", stream, x => new PostScriptTable(x)).Try();
        var os2 = ReadTableRecprds(font, "OS/2", stream, x => new OS2Table(x)).Try();
        var hhea = ReadTableRecprds(font, "hhea", stream, x => new HorizontalHeaderTable(x)).Try();
        var hmtx = ReadTableRecprds(font, "hmtx", stream, x => new HorizontalMetricsTable(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();
        var cff = ReadTableRecprds(font, "CFF ", stream, x => new CompactFontFormat(x));

        var cmap = font.TableRecords["cmap"];
        var encoding_records = EncodingRecord.ReadFrom(stream, cmap);
        var encoding =
            encoding_records.FirstOrDefault(x => x.PlatformEncoding == PlatformEncodings.UnicodeBMPOnly) ??
            encoding_records.FirstOrDefault(x => x.PlatformEncoding == PlatformEncodings.Windows_UnicodeBMP);
        var cmap4 = CMapFormat4.ReadFrom(stream, cmap, encoding.Try());
        var cmap4_range = new List<(int Start, int End)>();
        _ = cmap4.EndCode.Aggregate(0, (acc, x) =>
        {
            cmap4_range.Add((acc, x));
            return x + 1;
        });

        return new()
        {
            FontFamily = font.FontFamily,
            Style = font.Style,
            FullFontName = font.FullFontName,
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            NameRecords = font.NameRecords,
            FontHeader = head,
            MaximumProfile = maxp,
            PostScript = post,
            OS2 = os2,
            EncodingRecords = encoding_records,
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            HorizontalHeader = hhea,
            HorizontalMetrics = hmtx,
            CompactFontFormat = cff,
        };
    }

    public static T? ReadTableRecprds<T>(FontLoading font, string name, Stream stream, Func<Stream, T> f)
    {
        if (!font.TableRecords.TryGetValue(name, out var record)) return default;
        stream.Position = record.Offset;
        return f(stream);
    }
}

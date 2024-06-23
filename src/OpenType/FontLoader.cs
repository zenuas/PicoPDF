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
        var header = TrueTypeCollectionHeader.ReadFrom(stream);

        return header.TTCTag != "ttcf"
            ? throw new InvalidOperationException()
            : Enumerable.Range(0, (int)header.NumberOfFonts)
                .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
                .ToArray()
                .Select((x, i) => Load(new FontCollectionPath { Path = path, Index = i }, stream, x, opt))
                .ToArray();
    }

    public static FontLoading Load(IFontPath path, Stream stream, long pos, LoadOption opt)
    {
        stream.Position = pos;
        var offset = OffsetTable.ReadFrom(stream);
        if (!offset.ContainTrueType() && !offset.ContainCFF()) throw new InvalidOperationException();

        var tables = Enumerable.Range(0, offset.NumberOfTables)
            .Select(_ => TableRecord.ReadFrom(stream))
            .ToDictionary(x => x.TableTag, x => x);

        var namerecs = NameRecord.ReadFrom(stream, tables["name"]);
        string? namev(ushort nameid) => opt.PlatformIDOrder
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

    public static IOpenTypeRequiredTables DelayLoad(FontLoading font)
    {
        using var stream = File.OpenRead(font.Path.Path);

        var head = ReadTableRecprds(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecprds(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecprds(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var os2 = ReadTableRecprds(font, "OS/2", stream, OS2Table.ReadFrom).Try();
        var hhea = ReadTableRecprds(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var hmtx = ReadTableRecprds(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

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

        return font.Offset.ContainTrueType() ?
            LoadTrueTypeFont(
                stream,
                font,
                head,
                maxp,
                post,
                os2,
                hhea,
                hmtx,
                encoding_records,
                cmap4,
                cmap4_range
            ) :
            LoadPostScriptFont(
                stream,
                font,
                head,
                maxp,
                post,
                os2,
                hhea,
                hmtx,
                encoding_records,
                cmap4,
                cmap4_range
            );
    }

    public static TrueTypeFont LoadTrueTypeFont(
            Stream stream,
            FontLoading font,
            FontHeaderTable head,
            MaximumProfileTable maxp,
            PostScriptTable post,
            OS2Table os2,
            HorizontalHeaderTable hhea,
            HorizontalMetricsTable hmtx,
            EncodingRecord[] encoding_records,
            CMapFormat4 cmap4,
            List<(int Start, int End)> cmap4_range
        )
    {
        var loca = ReadTableRecprds(font, "loca", stream, x => IndexToLocationTable.ReadFrom(x, head.IndexToLocFormat, maxp.NumberOfGlyphs)).Try();

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
            IndexToLocation = loca,
        };
    }

    public static PostScriptFont LoadPostScriptFont(
            Stream stream,
            FontLoading font,
            FontHeaderTable head,
            MaximumProfileTable maxp,
            PostScriptTable post,
            OS2Table os2,
            HorizontalHeaderTable hhea,
            HorizontalMetricsTable hmtx,
            EncodingRecord[] encoding_records,
            CMapFormat4 cmap4,
            List<(int Start, int End)> cmap4_range
        )
    {
        var cff = ReadTableRecprds(font, "CFF ", stream, CompactFontFormat.ReadFrom).Try();

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

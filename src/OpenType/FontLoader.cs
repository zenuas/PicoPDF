﻿using Mina.Extension;
using PicoPDF.OpenType.PostScript;
using PicoPDF.OpenType.TrueType;
using System;
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
                .Select(_ => stream.ReadUIntByBigEndian())
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

        stream.Position = tables["name"].Offset;
        var namerecs = NameTable.ReadFrom(stream);
        string? namev(ushort nameid) => opt.PlatformIDOrder
            .Select(x => namerecs.NameRecords.FindFirstOrNullValue(y => y.NameRecord.PlatformID == x && y.NameRecord.NameID == nameid)?.Name)
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
            Name = namerecs,
        };
    }

    public static IOpenTypeRequiredTables DelayLoad(FontLoading font)
    {
        using var stream = File.OpenRead(font.Path.Path);

        var head = ReadTableRecprds(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecprds(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecprds(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var os2 = ReadTableRecprds(font, "OS/2", stream, OS2Table.ReadFrom).Try();
        var cmap = ReadTableRecprds(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var hhea = ReadTableRecprds(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var hmtx = ReadTableRecprds(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cmap_offset = font.TableRecords["cmap"].Offset;
        var cmap4 = cmap.EncodingRecords.Select(x =>
            {
                stream.Position = cmap_offset + x.Offset;
                return ReadCMapFormat(stream);
            })
            .OfType<CMapFormat4>()
            .First();
        var cmap4_range = CreateCMap4Range(cmap4);

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
                cmap,
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
                cmap,
                cmap4,
                cmap4_range
            );
    }

    public static ICMapFormat? ReadCMapFormat(Stream stream)
    {
        var format = stream.ReadUShortByBigEndian();
        return format switch
        {
            4 => CMapFormat4.ReadFrom(stream),
            _ => null,
        };
    }

    public static List<(int Start, int End)> CreateCMap4Range(CMapFormat4 cmap4)
    {
        var cmap4_range = new List<(int Start, int End)>();
        _ = cmap4.EndCode.Aggregate(0, (acc, x) =>
        {
            cmap4_range.Add((acc, x));
            return x + 1;
        });
        return cmap4_range;
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
            CMapTable cmap,
            CMapFormat4 cmap4,
            List<(int Start, int End)> cmap4_range
        )
    {
        var loca = ReadTableRecprds(font, "loca", stream, x => IndexToLocationTable.ReadFrom(x, head.IndexToLocFormat, maxp.NumberOfGlyphs)).Try();
        var glyf = ReadTableRecprds(font, "glyf", stream, x =>
            {
                var position = x.Position;
                return Enumerable.Range(0, maxp.NumberOfGlyphs)
                    .Select(i =>
                    {
                        x.Position = position + loca.Offsets[i];
                        var number_of_contours = x.ReadShortByBigEndian();
                        return number_of_contours >= 0
                            ? SimpleGlyph.ReadFrom(x, number_of_contours).Cast<IGlyph>()
                            : CompositeGlyph.ReadFrom(x, number_of_contours);
                    })
                    .ToArray();
            }).Try();

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
            Name = font.Name,
            FontHeader = head,
            MaximumProfile = maxp,
            PostScript = post,
            OS2 = os2,
            HorizontalHeader = hhea,
            HorizontalMetrics = hmtx,
            CMap = cmap,
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
            IndexToLocation = loca,
            Glyphs = glyf,
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
            CMapTable cmap,
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
            Name = font.Name,
            FontHeader = head,
            MaximumProfile = maxp,
            PostScript = post,
            OS2 = os2,
            HorizontalHeader = hhea,
            HorizontalMetrics = hmtx,
            CMap = cmap,
            CMap4 = cmap4,
            CMap4Range = cmap4_range,
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

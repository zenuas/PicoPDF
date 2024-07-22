using Mina.Extension;
using PicoPDF.OpenType.Tables;
using PicoPDF.OpenType.Tables.PostScript;
using PicoPDF.OpenType.Tables.TrueType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontLoader
{
    public static FontTableRecords LoadTableRecords(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadTableRecords(new FontPath { Path = path }, stream, opt ?? new());
    }

    public static FontTableRecords[] LoadTableRecordsCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadTableRecordsCollection(path, stream, opt ?? new());
    }

    public static FontTableRecords[] LoadTableRecordsCollection(string path, Stream stream, LoadOption opt)
    {
        var header = TrueTypeCollectionHeader.ReadFrom(stream);

        return header.TTCTag != "ttcf"
            ? throw new InvalidOperationException()
            : Enumerable.Repeat(0, (int)header.NumberOfFonts)
                .Select(_ => stream.ReadUIntByBigEndian())
                .ToArray()
                .Select((x, i) => LoadTableRecords(new FontCollectionPath { Path = path, Index = i }, stream.SeekTo(x), opt))
                .ToArray();
    }

    public static FontTableRecords LoadTableRecords(IFontPath path, Stream stream, LoadOption opt)
    {
        var position = stream.Position;
        var offset = OffsetTable.ReadFrom(stream);
        if (!offset.ContainTrueType() && !offset.ContainCFF()) throw new InvalidOperationException();

        var tables = Enumerable.Repeat(0, offset.NumberOfTables)
            .Select(_ => TableRecord.ReadFrom(stream))
            .ToDictionary(x => x.TableTag, x => x);

        var name = NameTable.ReadFrom(stream.SeekTo(tables["name"].Offset));
        string namev(NameIDs nameid) => opt.PlatformIDOrder
            .Select(x => name.NameRecords.FindFirstOrNullValue(y => y.NameRecord.PlatformID == (ushort)x && y.NameRecord.NameID == (ushort)nameid)?.Name)
            .Where(x => x is { })
            .FirstOrDefault() ?? "";

        return new()
        {
            PostScriptName = namev(NameIDs.PostScriptName),
            Path = path,
            Position = position,
            TableRecords = tables,
            Offset = offset,
            Name = name,
        };
    }

    public static FontRequiredTables LoadRequiredTables(FontTableRecords font)
    {
        using var stream = File.OpenRead(font.Path.Path);

        var head = ReadTableRecprds(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecprds(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecprds(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var os2 = ReadTableRecprds(font, "OS/2", stream, OS2Table.ReadFrom).Try();
        var cmap = ReadTableRecprds(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var hhea = ReadTableRecprds(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var hmtx = ReadTableRecprds(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cmap4 = cmap.EncodingRecords.Values.OfType<CMapFormat4>().First();
        var cmap4_range = CreateCMap4Range(cmap4);

        return new()
        {
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
        };
    }

    public static IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font) => font is FontRequiredTables x ? LoadComplete(x) : font;

    public static IOpenTypeRequiredTables LoadComplete(FontRequiredTables font)
    {
        using var stream = File.OpenRead(font.Path.Path);

        return font.Offset.ContainTrueType() ?
            LoadTrueTypeFont(stream, font) :
            LoadPostScriptFont(stream, font);
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

    public static TrueTypeFont LoadTrueTypeFont(Stream stream, FontRequiredTables font)
    {
        var loca = ReadTableRecprds(font, "loca", stream, x => IndexToLocationTable.ReadFrom(x, font.FontHeader.IndexToLocFormat, font.MaximumProfile.NumberOfGlyphs)).Try();
        var glyf = ReadTableRecprds(font, "glyf", stream, x =>
            {
                var position = x.Position;
                return Enumerable.Range(0, font.MaximumProfile.NumberOfGlyphs)
                    .Select(i =>
                    {
                        if (loca.Offsets[i] == loca.Offsets[i + 1]) return new NotdefGlyph();
                        var number_of_contours = x.SeekTo(position + loca.Offsets[i]).ReadShortByBigEndian();
                        return number_of_contours >= 0
                            ? SimpleGlyph.ReadFrom(x, number_of_contours).Cast<IGlyph>()
                            : CompositeGlyph.ReadFrom(x, number_of_contours);
                    })
                    .ToArray();
            }).Try();

        return new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = font.Name,
            FontHeader = font.FontHeader,
            MaximumProfile = font.MaximumProfile,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = font.HorizontalHeader,
            HorizontalMetrics = font.HorizontalMetrics,
            CMap = font.CMap,
            CMap4 = font.CMap4,
            CMap4Range = font.CMap4Range,
            IndexToLocation = loca,
            Glyphs = glyf,
        };
    }

    public static PostScriptFont LoadPostScriptFont(Stream stream, FontRequiredTables font)
    {
        var cff = ReadTableRecprds(font, "CFF ", stream, CompactFontFormat.ReadFrom).Try();

        return new()
        {
            PostScriptName = font.PostScriptName,
            Path = font.Path,
            Position = font.Position,
            TableRecords = font.TableRecords,
            Offset = font.Offset,
            Name = font.Name,
            FontHeader = font.FontHeader,
            MaximumProfile = font.MaximumProfile,
            PostScript = font.PostScript,
            OS2 = font.OS2,
            HorizontalHeader = font.HorizontalHeader,
            HorizontalMetrics = font.HorizontalMetrics,
            CMap = font.CMap,
            CMap4 = font.CMap4,
            CMap4Range = font.CMap4Range,
            CompactFontFormat = cff,
        };
    }

    public static T? ReadTableRecprds<T>(IOpenTypeHeader font, string name, Stream stream, Func<Stream, T> f)
    {
        if (!font.TableRecords.TryGetValue(name, out var record)) return default;
        return f(stream.SeekTo(record.Offset));
    }
}

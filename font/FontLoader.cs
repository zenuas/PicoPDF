using Mina.Extension;
using OpenType.Tables;
using OpenType.Tables.CMap;
using OpenType.Tables.PostScript;
using OpenType.Tables.TrueType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType;

public static class FontLoader
{
    public static FontTableRecords LoadTableRecords(string path, LoadOption? opt = null)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return LoadTableRecords(new FontPath { Path = path }, stream, opt ?? new());
    }

    public static FontTableRecords[] LoadTableRecordsCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return LoadTableRecordsCollection(path, stream, opt ?? new());
    }

    public static FontTableRecords[] LoadTableRecordsCollection(string path, Stream stream, LoadOption opt)
    {
        var header = TrueTypeCollectionHeader.ReadFrom(stream);

        return header.TTCTag != "ttcf"
            ? throw new InvalidDataException()
            : [.. header.TableDirectoryOffsets
                .Select((x, i) => Objects.Catch(() => LoadTableRecords(new FontCollectionPath { Path = path, Index = i }, stream.SeekTo(x), opt), out var r) is null ? r : null)
                .OfType<FontTableRecords>()];
    }

    public static FontTableRecords LoadTableRecords(IFontPath path, Stream stream, LoadOption opt)
    {
        var position = stream.Position;
        var offset = OffsetTable.ReadFrom(stream);
        if (!offset.ContainTrueType() && !offset.ContainCFF()) throw new InvalidDataException();

        var tables = Lists.Repeat(() => TableRecord.ReadFrom(stream))
            .Take(offset.NumberOfTables)
            .ToDictionary(x => x.TableTag, x => x);

        var name = ReadTableRecord(tables, "name", stream, NameTable.ReadFrom).Try();
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

    public static IOpenTypeFont LoadComplete(IOpenTypeHeader font) => font is IOpenTypeFont opentype ? opentype :
        font.Offset.ContainCFF() ? LoadPostScriptFont(font) :
        font.TableRecords.ContainsKey("loca") && font.TableRecords.ContainsKey("glyf") ? LoadTrueTypeFont(font) :
        LoadNoOutlineFont(font);

    public static TrueTypeFont LoadTrueTypeFont(IOpenTypeHeader font)
    {
        var stream = File.Open(font.Path.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var os2 = ReadTableRecord(font, "OS/2", stream, OS2Table.ReadFrom);
        var cmap = ReadTableRecord(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var head = ReadTableRecord(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var hhea = ReadTableRecord(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecord(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecord(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var hmtx = ReadTableRecord(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cbdt = ReadTableRecord(font, "CBDT", stream, ColorBitmapDataTable.ReadFrom);
        var cblc = ReadTableRecord(font, "CBLC", stream, ColorBitmapLocationTable.ReadFrom);
        var colr = ReadTableRecord(font, "COLR", stream, ColorTable.ReadFrom);
        var cpal = ReadTableRecord(font, "CPAL", stream, ColorPaletteTable.ReadFrom);
        var svg = ReadTableRecord(font, "SVG ", stream, ScalableVectorGraphicsTable.ReadFrom);
        var sbix = ReadTableRecord(font, "sbix", stream, StandardBitmapGraphicsTable.ReadFrom);

        var glyf = new LazyGlyph()
        {
            Stream = stream,
            Count = maxp.NumberOfGlyphs + 1,
            IndexToLocationTableOffset = font.TableRecords["loca"].Offset,
            GlyphTableOffset = font.TableRecords["glyf"].Offset,
            IndexToLocFormat = head.IndexToLocFormat,
        };

        var current_cmap = GetCurrentCMapFormat(cmap);
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
            CharToGID = current_cmap.CreateCharToGID(),
            GIDToOutline = gid => glyf[(int)gid].ToOutline(glyf),
            Glyphs = glyf,
            ColorBitmapData = cbdt,
            ColorBitmapLocation = cblc,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = sbix,
            ScalableVectorGraphics = svg,
            DisposeAction = () => glyf.Dispose(),
        };
    }

    public static PostScriptFont LoadPostScriptFont(IOpenTypeHeader font)
    {
        using var stream = File.Open(font.Path.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var os2 = ReadTableRecord(font, "OS/2", stream, OS2Table.ReadFrom);
        var cmap = ReadTableRecord(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var head = ReadTableRecord(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var hhea = ReadTableRecord(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecord(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecord(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var hmtx = ReadTableRecord(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cff = ReadTableRecord(font, "CFF ", stream, CompactFontFormat.ReadFrom).Try();

        var cbdt = ReadTableRecord(font, "CBDT", stream, ColorBitmapDataTable.ReadFrom);
        var cblc = ReadTableRecord(font, "CBLC", stream, ColorBitmapLocationTable.ReadFrom);
        var colr = ReadTableRecord(font, "COLR", stream, ColorTable.ReadFrom);
        var cpal = ReadTableRecord(font, "CPAL", stream, ColorPaletteTable.ReadFrom);
        var svg = ReadTableRecord(font, "SVG ", stream, ScalableVectorGraphicsTable.ReadFrom);
        var sbix = ReadTableRecord(font, "sbix", stream, StandardBitmapGraphicsTable.ReadFrom);

        var current_cmap = GetCurrentCMapFormat(cmap);
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
            CharToGID = current_cmap.CreateCharToGID(),
            GIDToOutline = _ => [],
            CompactFontFormat = cff,
            ColorBitmapData = cbdt,
            ColorBitmapLocation = cblc,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = sbix,
            ScalableVectorGraphics = svg,
        };
    }

    public static NoOutlineFont LoadNoOutlineFont(IOpenTypeHeader font)
    {
        using var stream = File.Open(font.Path.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var os2 = ReadTableRecord(font, "OS/2", stream, OS2Table.ReadFrom);
        var cmap = ReadTableRecord(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var head = ReadTableRecord(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var hhea = ReadTableRecord(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecord(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecord(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var hmtx = ReadTableRecord(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cbdt = ReadTableRecord(font, "CBDT", stream, ColorBitmapDataTable.ReadFrom);
        var cblc = ReadTableRecord(font, "CBLC", stream, ColorBitmapLocationTable.ReadFrom);
        var colr = ReadTableRecord(font, "COLR", stream, ColorTable.ReadFrom);
        var cpal = ReadTableRecord(font, "CPAL", stream, ColorPaletteTable.ReadFrom);
        var svg = ReadTableRecord(font, "SVG ", stream, ScalableVectorGraphicsTable.ReadFrom);
        var sbix = ReadTableRecord(font, "sbix", stream, StandardBitmapGraphicsTable.ReadFrom);

        var current_cmap = GetCurrentCMapFormat(cmap);
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
            CharToGID = current_cmap.CreateCharToGID(),
            GIDToOutline = _ => [],
            ColorBitmapData = cbdt,
            ColorBitmapLocation = cblc,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = sbix,
            ScalableVectorGraphics = svg,
        };
    }

    public static ICMapFormat GetCurrentCMapFormat(CMapTable cmap) =>
        (ICMapFormat?)cmap.EncodingRecords.Values.OfType<CMapFormat12>().FirstOrDefault() ??
        (ICMapFormat?)cmap.EncodingRecords.Values.OfType<CMapFormat4>().FirstOrDefault() ??
        (ICMapFormat?)cmap.EncodingRecords.Values.OfType<CMapFormat0>().FirstOrDefault() ??
        cmap.EncodingRecords.Values.OfType<CMapFormat13>().First();

    public static T? ReadTableRecord<T>(IOpenTypeHeader font, string name, Stream stream, Func<Stream, T> f) => ReadTableRecord(font.TableRecords, name, stream, f);

    public static T? ReadTableRecord<T>(Dictionary<string, TableRecord> tables, string name, Stream stream, Func<Stream, T> f) => !tables.TryGetValue(name, out var record) ? default : f(stream.SeekTo(record.Offset));
}

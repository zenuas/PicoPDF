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
            : [.. Lists.Repeat(() => stream.ReadUIntByBigEndian())
                .Take((int)header.NumberOfFonts)
                .ToArray()
                .Select((x, i) => Objects.Catch(() => LoadTableRecords(new FontCollectionPath { Path = path, Index = i }, stream.SeekTo(x), opt), out var r) is null ? r : null)
                .OfType<FontTableRecords>()];
    }

    public static FontTableRecords LoadTableRecords(IFontPath path, Stream stream, LoadOption opt)
    {
        var position = stream.Position;
        var offset = OffsetTable.ReadFrom(stream);
        if (!offset.ContainTrueType() && !offset.ContainCFF()) throw new InvalidDataException();

        var tables = Enumerable.Repeat(0, offset.NumberOfTables)
            .Select(_ => TableRecord.ReadFrom(stream))
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

    public static FontRequiredTables LoadRequiredTables(FontTableRecords font)
    {
        using var stream = File.Open(font.Path.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var os2 = ReadTableRecord(font, "OS/2", stream, OS2Table.ReadFrom);
        var cmap = ReadTableRecord(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var head = ReadTableRecord(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var hhea = ReadTableRecord(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecord(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecord(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var hmtx = ReadTableRecord(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var current_cmap =
            (ICMapFormat?)cmap.EncodingRecords.Values.OfType<CMapFormat12>().FirstOrDefault() ??
            (ICMapFormat?)cmap.EncodingRecords.Values.OfType<CMapFormat4>().FirstOrDefault() ??
            cmap.EncodingRecords.Values.OfType<CMapFormat0>().FirstOrDefault();

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
            CharToGID = current_cmap!.CreateCharToGID(),
        };
    }

    public static IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font) => font is FontRequiredTables x ? LoadComplete(x) : font;

    public static IOpenTypeRequiredTables LoadComplete(FontRequiredTables font)
    {
        using var stream = File.Open(font.Path.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        return font.Offset.ContainTrueType()
            ? LoadTrueTypeFont(stream, font)
            : LoadPostScriptFont(stream, font);
    }

    public static TrueTypeFont LoadTrueTypeFont(Stream stream, FontRequiredTables font)
    {
        var loca = ReadTableRecord(font, "loca", stream, x => IndexToLocationTable.ReadFrom(x, font.FontHeader.IndexToLocFormat, font.MaximumProfile.NumberOfGlyphs)).Try();
        var glyf = ReadTableRecord(font, "glyf", stream, x =>
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

        var cbdt = ReadTableRecord(font, "CBDT", stream, ColorBitmapDataTable.ReadFrom);
        var cblc = ReadTableRecord(font, "CBLC", stream, ColorBitmapLocationTable.ReadFrom);
        var colr = ReadTableRecord(font, "COLR", stream, ColorTable.ReadFrom);
        var cpal = ReadTableRecord(font, "CPAL", stream, ColorPaletteTable.ReadFrom);
        var svg = ReadTableRecord(font, "SVG ", stream, ScalableVectorGraphicsTable.ReadFrom);
        var sbix = ReadTableRecord(font, "sbix", stream, StandardBitmapGraphicsTable.ReadFrom);

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
            CharToGID = font.CharToGID,
            IndexToLocation = loca,
            Glyphs = glyf,
            ColorBitmapData = cbdt,
            ColorBitmapLocation = cblc,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = sbix,
            ScalableVectorGraphics = svg,
        };
    }

    public static PostScriptFont LoadPostScriptFont(Stream stream, FontRequiredTables font)
    {
        var cff = ReadTableRecord(font, "CFF ", stream, CompactFontFormat.ReadFrom).Try();

        var cbdt = ReadTableRecord(font, "CBDT", stream, ColorBitmapDataTable.ReadFrom);
        var cblc = ReadTableRecord(font, "CBLC", stream, ColorBitmapLocationTable.ReadFrom);
        var colr = ReadTableRecord(font, "COLR", stream, ColorTable.ReadFrom);
        var cpal = ReadTableRecord(font, "CPAL", stream, ColorPaletteTable.ReadFrom);
        var svg = ReadTableRecord(font, "SVG ", stream, ScalableVectorGraphicsTable.ReadFrom);
        var sbix = ReadTableRecord(font, "sbix", stream, StandardBitmapGraphicsTable.ReadFrom);

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
            CharToGID = font.CharToGID,
            CompactFontFormat = cff,
            ColorBitmapData = cbdt,
            ColorBitmapLocation = cblc,
            Color = colr,
            ColorPalette = cpal,
            StandardBitmapGraphics = sbix,
            ScalableVectorGraphics = svg,
        };
    }

    public static T? ReadTableRecord<T>(IOpenTypeHeader font, string name, Stream stream, Func<Stream, T> f) => ReadTableRecord(font.TableRecords, name, stream, f);

    public static T? ReadTableRecord<T>(Dictionary<string, TableRecord> tables, string name, Stream stream, Func<Stream, T> f) => !tables.TryGetValue(name, out var record) ? default : f(stream.SeekTo(record.Offset));
}

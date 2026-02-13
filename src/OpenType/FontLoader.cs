using Mina.Extension;
using PicoPDF.OpenType.Tables;
using PicoPDF.OpenType.Tables.PostScript;
using PicoPDF.OpenType.Tables.TrueType;
using System;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontLoader
{
    public static FontTableRecords LoadTableRecords(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadTableRecords(new FontPath { Path = path, ForceEmbedded = opt?.ForceEmbedded ?? false }, stream, opt ?? new());
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
            : [.. Enumerable.Repeat(0, (int)header.NumberOfFonts)
                .Select(_ => stream.ReadUIntByBigEndian())
                .ToArray()
                .Select((x, i) => Objects.Catch(() => LoadTableRecords(new FontCollectionPath { Path = path, Index = i, ForceEmbedded = opt.ForceEmbedded }, stream.SeekTo(x), opt), out var r) is null ? r : null)
                .OfType<FontTableRecords>()];
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

        var head = ReadTableRecord(font, "head", stream, FontHeaderTable.ReadFrom).Try();
        var maxp = ReadTableRecord(font, "maxp", stream, MaximumProfileTable.ReadFrom).Try();
        var post = ReadTableRecord(font, "post", stream, PostScriptTable.ReadFrom).Try();
        var os2 = ReadTableRecord(font, "OS/2", stream, OS2Table.ReadFrom).Try();
        var cmap = ReadTableRecord(font, "cmap", stream, CMapTable.ReadFrom).Try();
        var hhea = ReadTableRecord(font, "hhea", stream, HorizontalHeaderTable.ReadFrom).Try();
        var hmtx = ReadTableRecord(font, "hmtx", stream, x => HorizontalMetricsTable.ReadFrom(x, hhea.NumberOfHMetrics, maxp.NumberOfGlyphs)).Try();

        var cmap4 = cmap.EncodingRecords.Values.OfType<CMapFormat4>().First();

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
            CharToGID = cmap4.CreateCharToGID(),
        };
    }

    public static IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font) => font is FontRequiredTables x ? LoadComplete(x) : font;

    public static IOpenTypeRequiredTables LoadComplete(FontRequiredTables font)
    {
        using var stream = File.OpenRead(font.Path.Path);

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
        };
    }

    public static PostScriptFont LoadPostScriptFont(Stream stream, FontRequiredTables font)
    {
        var cff = ReadTableRecord(font, "CFF ", stream, CompactFontFormat.ReadFrom).Try();

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
        };
    }

    public static T? ReadTableRecord<T>(IOpenTypeHeader font, string name, Stream stream, Func<Stream, T> f) => !font.TableRecords.TryGetValue(name, out var record) ? default : f(stream.SeekTo(record.Offset));
}

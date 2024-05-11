using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public static class TrueTypeFontLoader
{
    public static TrueTypeFontInfo Load(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return Load(path, stream, 0, opt ?? new());
    }

    public static TrueTypeFontInfo[] LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadCollection(path, stream, opt ?? new());
    }

    public static TrueTypeFontInfo[] LoadCollection(string path, Stream stream, LoadOption opt)
    {
        var header = new TrueTypeCollectionHeader(stream);
        if (header.TTCTag != "ttcf") throw new InvalidOperationException();

        return Enumerable.Range(0, (int)header.NumberOfFonts)
            .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
            .ToArray()
            .Select(x => Load(path, stream, x, opt))
            .ToArray();
    }

    public static TrueTypeFontInfo Load(string path, Stream stream, long pos, LoadOption opt)
    {
        stream.Position = pos;
        var header = new OffsetTable(stream);
        if (header.Version != 0x00010000 && header.Version != 0x4F54544F) throw new InvalidOperationException();

        var tables = Enumerable.Range(0, header.NumberOfTables)
            .Select(_ => new TableRecord(stream))
            .ToDictionary(x => x.TableTag, x => x);

        var namerecs = NameRecord.ReadFrom(stream, tables["name"]);
        var namev = (ushort nameid) => opt.PlatformIDOrder
            .Select(x => namerecs.FindFirstOrNullValue(y => y.NameRecord.PlatformID == x && y.NameRecord.NameID == nameid)?.Name)
            .Where(x => x is { })
            .FirstOrDefault();

        return new TrueTypeFontInfo()
        {
            FontFamily = namev(1) ?? "",
            Style = namev(2) ?? "",
            FullFontName = namev(4) ?? "",
            PostScriptName = namev(6) ?? "",
            Path = path,
            Position = pos,
            TableRecords = tables,
        };
    }

    public static void DelayLoad(TrueTypeFontInfo font)
    {
        using var stream = File.OpenRead(font.Path);

        stream.Position = font.TableRecords["head"].Offset;
        font.FontHeader = new FontHeaderTable(stream);

        stream.Position = font.TableRecords["maxp"].Offset;
        font.MaximumProfile = new MaximumProfileTable(stream);

        stream.Position = font.TableRecords["OS/2"].Offset;
        font.OS2 = new OS2Table(stream);

        var cmap = font.TableRecords["cmap"];
        var encodings = EncodingRecord.ReadFrom(stream, cmap);
        var encoding =
            encodings.FirstOrDefault(x => x.PlatformID == 0 && x.EncodingID == 3) ??
            encodings.FirstOrDefault(x => x.PlatformID == 3 && x.EncodingID == 1);
        if (encoding is { })
        {
            font.CMap4 = new CMapFormat4(stream, cmap, encoding);
            _ = font.CMap4.EndCode.Aggregate(0, (acc, x) =>
            {
                font.CMap4Range.Add((acc, x));
                return x + 1;
            });
        }

        stream.Position = font.TableRecords["hhea"].Offset;
        font.HorizontalHeader = new HorizontalHeaderTable(stream);

        stream.Position = font.TableRecords["hmtx"].Offset;
        font.HorizontalMetrics = new HorizontalMetricsTable(stream, font.HorizontalHeader.NumberOfHMetrics, font.MaximumProfile.NumberOfGlyphs);

        font.Loaded = true;
    }
}

﻿using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.TrueType;

public class TrueTypeFontLoader
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
        var header = TrueTypeCollectionHeader.ReadFrom(stream);
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
        var header = OffsetTable.ReadFrom(stream);
        if (header.Version != 0x00010000 && header.Version != 0x4F54544F) throw new InvalidOperationException();

        var tables = Enumerable.Range(0, header.NumberOfTables)
            .Select(_ => TableRecord.ReadFrom(stream))
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
        font.FontHeader = FontHeaderTable.ReadFrom(stream);

        stream.Position = font.TableRecords["maxp"].Offset;
        font.MaximumProfile = MaximumProfileTable.ReadFrom(stream);

        stream.Position = font.TableRecords["OS/2"].Offset;
        font.OS2 = OS2Table.ReadFrom(stream);

        var cmap = font.TableRecords["cmap"];
        var encodings = EncodingRecord.ReadFrom(stream, cmap);
        var encoding =
            encodings.FindFirstOrNullValue(x => x.PlatformID == 0 && x.EncodingID == 3) ??
            encodings.FindFirstOrNullValue(x => x.PlatformID == 3 && x.EncodingID == 1);
        if (encoding.HasValue) font.CMap4 = CMapFormat4.ReadFrom(stream, cmap, encoding.Value);

        stream.Position = font.TableRecords["hhea"].Offset;
        font.HorizontalHeader = HorizontalHeaderTable.ReadFrom(stream);

        stream.Position = font.TableRecords["hmtx"].Offset;
        font.HorizontalMetrics = HorizontalMetricsTable.ReadFrom(stream, font.HorizontalHeader.NumberOfHMetrics, font.MaximumProfile.NumberOfGlyphs);

        font.Loaded = true;
    }
}

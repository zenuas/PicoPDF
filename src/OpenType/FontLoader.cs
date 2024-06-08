﻿using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public static class FontLoader
{
    public static FontInfo Load(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return Load(path, stream, 0, opt ?? new());
    }

    public static FontInfo[] LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadCollection(path, stream, opt ?? new());
    }

    public static FontInfo[] LoadCollection(string path, Stream stream, LoadOption opt)
    {
        var header = new TrueTypeCollectionHeader(stream);
        if (header.TTCTag != "ttcf") throw new InvalidOperationException();

        return Enumerable.Range(0, (int)header.NumberOfFonts)
            .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
            .ToArray()
            .Select(x => Load(path, stream, x, opt))
            .ToArray();
    }

    public static FontInfo Load(string path, Stream stream, long pos, LoadOption opt)
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
            Offset = offset,
            FontFamily = namev(1) ?? "",
            Style = namev(2) ?? "",
            FullFontName = namev(4) ?? "",
            PostScriptName = namev(6) ?? "",
            Path = path,
            Position = pos,
            TableRecords = tables,
        };
    }

    public static void DelayLoad(FontInfo font)
    {
        using var stream = File.OpenRead(font.Path);

        stream.Position = font.TableRecords["head"].Offset;
        font.FontHeader = new(stream);

        stream.Position = font.TableRecords["maxp"].Offset;
        font.MaximumProfile = new(stream);

        stream.Position = font.TableRecords["OS/2"].Offset;
        font.OS2 = new(stream);

        var cmap = font.TableRecords["cmap"];
        var encodings = EncodingRecord.ReadFrom(stream, cmap);
        var encoding =
            encodings.FirstOrDefault(x => x.PlatformID == 0 && x.EncodingID == 3) ??
            encodings.FirstOrDefault(x => x.PlatformID == 3 && x.EncodingID == 1);
        if (encoding is { })
        {
            font.CMap4 = new(stream, cmap, encoding);
            _ = font.CMap4.EndCode.Aggregate(0, (acc, x) =>
            {
                font.CMap4Range.Add((acc, x));
                return x + 1;
            });
        }

        stream.Position = font.TableRecords["hhea"].Offset;
        font.HorizontalHeader = new(stream);

        stream.Position = font.TableRecords["hmtx"].Offset;
        font.HorizontalMetrics = new(stream, font.HorizontalHeader.NumberOfHMetrics, font.MaximumProfile.NumberOfGlyphs);

        if (font.TableRecords.TryGetValue("CFF ", out var cff))
        {
            stream.Position = cff.Offset;
            font.CompactFontFormat = new(stream);
        }

        font.Loaded = true;
    }
}
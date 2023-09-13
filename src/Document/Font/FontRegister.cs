using Extensions;
using PicoPDF.Document.Font.TrueType;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Document.Font;

public class FontRegister
{
    public Dictionary<string, TrueTypeFont> Fonts { get; init; } = new();

    public void RegistDirectory(string path)
    {
        Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
            .Where(x => x.Extension.In(".TTF", ".TTC"))
            .Select(x => x.Extension == ".TTF" ? [Load(x.Path)] : LoadCollection(x.Path))
            .Flatten()
            .Each(x => Fonts.TryAdd(x.PostScriptName, x));
    }

    public static TrueTypeFont Load(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return Load(path, stream, 0, opt ?? new());
    }

    public static TrueTypeFont[] LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadCollection(path, stream, opt ?? new());
    }

    public static TrueTypeFont[] LoadCollection(string path, Stream stream, LoadOption opt)
    {
        var header = TTCHeader.ReadFrom(stream);
        if (header.TTCTag != "ttcf") throw new InvalidOperationException();

        return Enumerable.Range(0, (int)header.NumberOfFonts)
            .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
            .ToArray()
            .Select(x => Load(path, stream, x, opt))
            .ToArray();
    }

    public static TrueTypeFont Load(string path, Stream stream, long pos, LoadOption opt)
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

        return new TrueTypeFont()
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

    public TrueTypeFont? GetOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null || font.Loaded) return font;

        DelayLoad(font);
        return font;
    }

    public static void DelayLoad(TrueTypeFont font)
    {
        using var stream = File.OpenRead(font.Path);
        stream.Position = font.Position;
        font.FontHeader = FontHeaderTable.ReadFrom(new MemoryStream(stream.ReadPositionBytes(font.TableRecords["head"].Offset, (int)font.TableRecords["head"].Length)));

        var cmap = font.TableRecords["cmap"];
        var encodings = EncodingRecord.ReadFrom(stream, cmap);
        var encoding =
            encodings.FindFirstOrNullValue(x => x.PlatformID == 0 && x.EncodingID == 3) ??
            encodings.FindFirstOrNullValue(x => x.PlatformID == 3 && x.EncodingID == 1);
        if (encoding.HasValue) font.CMap4 = CMapFormat4.ReadFrom(stream, cmap, encoding.Value);

        font.Loaded = true;
    }
}

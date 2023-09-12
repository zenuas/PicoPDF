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
        return Load(stream, opt ?? new());
    }

    public static TrueTypeFont[] LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return LoadCollection(stream, opt ?? new());
    }

    public static TrueTypeFont[] LoadCollection(Stream stream, LoadOption opt)
    {
        var header = TTCHeader.ReadFrom(stream);
        if (header.TTCTag != "ttcf") throw new InvalidOperationException();

        return Enumerable.Range(0, (int)header.NumberOfFonts)
            .Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)))
            .ToArray()
            .Select(x => { stream.Position = x; return Load(stream, opt); })
            .ToArray();
    }

    public static TrueTypeFont Load(Stream stream, LoadOption opt)
    {
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
            FontHeader = FontHeaderTable.ReadFrom(new MemoryStream(stream.ReadPositionBytes(tables["head"].Offset, (int)tables["head"].Length))),
        };
    }
}

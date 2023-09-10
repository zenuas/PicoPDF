using Extensions;
using PicoPDF.Document.Font.TrueType;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.Document.Font;

public class TrueTypeFont : PdfObject, IFont
{
    public required string Name { get; init; }
    public required string Style { get; init; }
    public required string FontFamily { get; init; }
    public required Encoding TextEncoding { get; init; }

    public static TrueTypeFont Load(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        return Load(stream, opt ?? new());
    }

    public static IEnumerable<TrueTypeFont> LoadCollection(string path, LoadOption? opt = null)
    {
        using var stream = File.OpenRead(path);
        var head = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
        if (head != 0x74746366) throw new InvalidOperationException();
        yield break;
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
        Debug.Assert(namev(4) is { });

        return new TrueTypeFont()
        {
            Name = namev(6) ?? "",
            Style = namev(2) ?? "",
            FontFamily = namev(1) ?? "",
            TextEncoding = Encoding.ASCII,
        };
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return TextEncoding.GetBytes(s);
    }
}
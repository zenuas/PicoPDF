using Mina.Binder;
using Mina.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class CMapFormat12 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Reserved { get; init; }
    public required uint Length { get; init; }
    public required uint Language { get; init; }
    public required uint NumberOfGroups { get; init; }
    public required (uint StartCharCode, uint EndCharCode, uint StartGlyphID)[] Groups { get; init; }

    public static CMapFormat12 ReadFrom(Stream stream)
    {
        var reserved = stream.ReadUShortByBigEndian();
        var length = stream.ReadUIntByBigEndian();
        var language = stream.ReadUIntByBigEndian();
        var num_of_groups = stream.ReadUIntByBigEndian();

        return new()
        {
            Format = 12,
            Reserved = reserved,
            Length = length,
            Language = language,
            NumberOfGroups = num_of_groups,
            Groups = [.. Enumerable.Repeat(0, (int)num_of_groups).Select(_ => (stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian()))],
        };
    }

    public static CMapFormat12 CreateFormat(Dictionary<char, uint> char_gid)
    {
        var chars = char_gid.Keys.Order().ToArray();
        var groups = CreateStartEnds(chars, char_gid);

        return new()
        {
            Format = 12,
            Reserved = 0,
            Length = (uint)(16 + groups.Length * 12), // sizeof(Format) + sizeof(Reserved) + sizeof(Length) + sizeof(Language) + sizeof(NumberOfGroups) + NumberOfGroups * sizeof(Groups)
            Language = 0,
            NumberOfGroups = (uint)groups.Length,
            Groups = groups,
        };
    }

    public static (uint StartCharCode, uint EndCharCode, uint StartGlyphID)[] CreateStartEnds(Span<char> chars, Dictionary<char, uint> char_gid)
    {
        var start = chars[0];
        var gid = char_gid[chars[0]];
        if (chars.Length == 1) return [(start, start, gid)];

        for (var i = 1; i < chars.Length; i++)
        {
            if (start + i == chars[i] && gid + i == char_gid[chars[i]]) continue;
            return [(start, (uint)(start + i), gid), .. CreateStartEnds(chars[i..], char_gid)];
        }
        return [(start, chars[^1], gid)];
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Reserved);
        stream.WriteUIntByBigEndian(Length);
        stream.WriteUIntByBigEndian(Language);
        stream.WriteUIntByBigEndian(NumberOfGroups);
        Groups.Each(x =>
        {
            stream.WriteUIntByBigEndian(x.StartCharCode);
            stream.WriteUIntByBigEndian(x.EndCharCode);
            stream.WriteUIntByBigEndian(x.StartGlyphID);
        });
    }

    public static readonly ComparerBinder<(uint StartCharCode, uint EndCharCode, uint StartGlyphID)> RangeComparer = new() { Compare = (a, b) => a.StartCharCode < b.StartCharCode ? -1 : a.EndCharCode > b.EndCharCode ? 1 : 0 };

    public Func<char, uint> CreateCharToGID()
    {
        return (c) =>
        {
            var code = (uint)c;
            var index = Groups.BinarySearch((code, code, 0u), RangeComparer);
            if (index < 0) return 0;
            var (start_code, _, start_gid) = Groups[index];
            return start_gid + (code - start_code);
        };
    }
}

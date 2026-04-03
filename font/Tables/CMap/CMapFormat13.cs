using Mina.Binder;
using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat13 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Reserved { get; init; }
    public required uint Length { get; init; }
    public required uint Language { get; init; }
    public required uint NumberOfGroups { get; init; }
    public required (uint StartCharCode, uint EndCharCode, uint GlyphID)[] Groups { get; init; }

    public static CMapFormat13 ReadFrom(Stream stream)
    {
        var reserved = stream.ReadUShortByBigEndian();
        var length = stream.ReadUIntByBigEndian();
        var language = stream.ReadUIntByBigEndian();
        var num_of_groups = stream.ReadUIntByBigEndian();

        return new()
        {
            Format = (ushort)CMapFormats.Format13,
            Reserved = reserved,
            Length = length,
            Language = language,
            NumberOfGroups = num_of_groups,
            Groups = [.. Lists.Repeat(() => (stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian())).Take((int)num_of_groups)],
        };
    }

    public static CMapFormat13 CreateFormat((int Char, uint GID)[] char_gids)
    {
        var groups = CreateStartEnds(char_gids);

        return new()
        {
            Format = 13,
            Reserved = 0,
            Length = (uint)(/* sizeof(Format) + sizeof(Reserved) + sizeof(Length) + sizeof(Language) + sizeof(NumberOfGroups) */16 + (groups.Length * /* sizeof(Groups) */12)),
            Language = 0,
            NumberOfGroups = (uint)groups.Length,
            Groups = groups,
        };
    }

    public static (uint StartCharCode, uint EndCharCode, uint StartGlyphID)[] CreateStartEnds(Span<(int Char, uint GID)> char_gids)
    {
        if (char_gids.Length == 0) return [];
        var (start, gid) = char_gids[0];
        if (char_gids.Length == 1) return [((uint)start, (uint)start, gid)];

        for (var i = 1; i < char_gids.Length; i++)
        {
            if (start + i == char_gids[i].Char && gid == char_gids[i].GID) continue;
            return [((uint)start, (uint)(start + i - 1), gid), .. CreateStartEnds(char_gids[i..])];
        }
        return [((uint)start, (uint)char_gids[^1].Char, gid)];
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Reserved);
        stream.WriteUIntByBigEndian(Length);
        stream.WriteUIntByBigEndian(Language);
        stream.WriteUIntByBigEndian(NumberOfGroups);
        foreach (var x in Groups)
        {
            stream.WriteUIntByBigEndian(x.StartCharCode);
            stream.WriteUIntByBigEndian(x.EndCharCode);
            stream.WriteUIntByBigEndian(x.GlyphID);
        }
    }

    public static readonly ComparerBinder<(uint StartCharCode, uint EndCharCode, uint StartGlyphID)> RangeComparer = new() { Compare = (a, b) => a.StartCharCode < b.StartCharCode ? -1 : a.EndCharCode > b.EndCharCode ? 1 : 0 };

    public Func<int, uint> CreateCharToGID()
    {
        return (c) =>
        {
            var code = (uint)c;
            var index = Groups.BinarySearch((code, code, 0u), RangeComparer);
            if (index < 0) return 0;
            var (_, _, gid) = Groups[index];
            return gid;
        };
    }

    public Func<uint, int?> CreateGIDToChar()
    {
        return (gid) => (int?)(Groups.FindFirstOrNullValue(x => x.GlyphID == gid)?.StartCharCode);
    }
}

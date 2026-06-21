using Mina.Binder;
using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat8 : ICMapFormat, ICharToGID
{
    public required ushort Format { get; init; }
    public required ushort Reserved { get; init; }
    public required uint Length { get; init; }
    public required uint Language { get; init; }
    public required byte[] Is32 { get; init; }
    public required uint NumberOfGroups { get; init; }
    public required (uint StartCharCode, uint EndCharCode, uint StartGlyphID)[] Groups { get; init; }

    public static CMapFormat8 ReadFrom(Stream stream)
    {
        var reserved = stream.ReadUShortByBigEndian();
        var length = stream.ReadUIntByBigEndian();
        var language = stream.ReadUIntByBigEndian();
        var is32 = Lists.Repeat(stream.ReadUByte).Take(8192).ToArray();
        var num_of_groups = stream.ReadUIntByBigEndian();

        return new()
        {
            Format = 8,
            Reserved = reserved,
            Length = length,
            Language = language,
            Is32 = is32,
            NumberOfGroups = num_of_groups,
            Groups = [.. Lists.Repeat(() => (stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian(), stream.ReadUIntByBigEndian())).Take((int)num_of_groups)],
        };
    }

    public static readonly ComparerBinder<(uint StartCharCode, uint EndCharCode, uint StartGlyphID)> RangeComparer = new() { Compare = (a, b) => a.EndCharCode < b.StartCharCode ? -1 : a.StartCharCode > b.EndCharCode ? 1 : 0 };

    public Func<int, uint> CreateCharToGID()
    {
        return (c) =>
        {
            var code = (uint)c;
            var index = Groups.BinarySearch((code, code, 0u), RangeComparer);
            if (index < 0) return 0;
            var g = Groups[index];
            return g.StartGlyphID + (code - g.StartCharCode);
        };
    }
}

using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat10 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Reserved { get; init; }
    public required uint Length { get; init; }
    public required uint Language { get; init; }
    public required uint StartCharCode { get; init; }
    public required uint NumberOfChars { get; init; }
    public required ushort[] GlyphIdArray { get; init; }

    public static CMapFormat10 ReadFrom(Stream stream)
    {
        var reserved = stream.ReadUShortByBigEndian();
        var length = stream.ReadUIntByBigEndian();
        var language = stream.ReadUIntByBigEndian();
        var startcharcode = stream.ReadUIntByBigEndian();
        var num_of_chars = stream.ReadUIntByBigEndian();

        return new()
        {
            Format = 10,
            Reserved = reserved,
            Length = length,
            Language = language,
            StartCharCode = startcharcode,
            NumberOfChars = num_of_chars,
            GlyphIdArray = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take((int)num_of_chars)],
        };
    }

    public Func<int, uint> CreateCharToGID() => (c) => c < StartCharCode || c >= StartCharCode + NumberOfChars ? 0 : (uint)GlyphIdArray[c - StartCharCode];
}

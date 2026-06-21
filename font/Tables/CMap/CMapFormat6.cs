using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat6 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required ushort FirstCode { get; init; }
    public required ushort EntryCount { get; init; }
    public required ushort[] GlyphIdArray { get; init; }

    public static CMapFormat6 ReadFrom(Stream stream)
    {
        var length = stream.ReadUShortByBigEndian();
        var language = stream.ReadUShortByBigEndian();
        var firstcode = stream.ReadUShortByBigEndian();
        var entrycount = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 6,
            Length = length,
            Language = language,
            FirstCode = firstcode,
            EntryCount = entrycount,
            GlyphIdArray = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take(entrycount)],
        };
    }

    public Func<int, uint> CreateCharToGID() => (c) => c < FirstCode || c >= FirstCode + EntryCount ? 0 : (uint)GlyphIdArray[c - FirstCode];
}

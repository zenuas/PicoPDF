using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat2 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required ushort[] SubHeaderKeys { get; init; }
    public required (ushort FirstCode, ushort EntryCount, short IdDelta, ushort IdRangeOffset)[] SubHeaders { get; init; }
    public required ushort[] GlyphIdArray { get; init; }

    public static CMapFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var length = stream.ReadUShortByBigEndian();
        var language = stream.ReadUShortByBigEndian();
        var subheader_keys = Lists.Repeat(stream.ReadUShortByBigEndian).Take(256).ToArray();
        var subheaders = Lists.Repeat(() => (stream.ReadUShortByBigEndian(), stream.ReadUShortByBigEndian(), stream.ReadShortByBigEndian(), stream.ReadUShortByBigEndian())).Take((subheader_keys.Max() / 8) + 1).ToArray();

        return new()
        {
            Format = 2,
            Length = length,
            Language = language,
            SubHeaderKeys = subheader_keys,
            SubHeaders = subheaders,
            GlyphIdArray = [.. Lists.Repeat(stream.ReadUShortByBigEndian).Take((int)((length - (stream.Position - position - /* sizeof(Format) */2)) / 2))],
        };
    }

    public void WriteTo(Stream stream) => throw new NotImplementedException();

    public Func<int, uint> CreateCharToGID()
    {
        return (c) =>
        {
            var subheader_index = SubHeaderKeys[(c >> 8) & 0xFF] / 8;
            var low = c & 0xFF;
            var sub = SubHeaders[subheader_index];

            if (low < sub.FirstCode || low >= sub.FirstCode + sub.EntryCount) return 0;

            var glyph_index = (sub.IdRangeOffset / 2) + (low - sub.FirstCode) - (SubHeaders.Length - subheader_index);
            if (glyph_index < 0 || glyph_index >= GlyphIdArray.Length) return 0;

            var gid = GlyphIdArray[glyph_index];
            return (uint)(gid == 0 ? 0 : (gid + sub.IdDelta) & 0xFFFF);
        };
    }
}

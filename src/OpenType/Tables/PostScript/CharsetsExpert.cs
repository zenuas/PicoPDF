using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CharsetsExpert : ICharsets
{
    public required byte Format { get; init; }
    public required ushort[] Glyph { get; init; }

    public static CharsetsExpert ReadFrom(Stream stream, int glyph_count)
    {
        var glyph = new List<ushort>();
        while (glyph.Count < glyph_count)
        {
            var first = stream.ReadUShortByBigEndian();
            var left = stream.ReadUByte();
            Enumerable.Repeat(0, left + 1).Each(x => glyph.Add((ushort)(first + x)));
        }

        return new()
        {
            Format = 1,
            Glyph = [.. glyph],
        };
    }
}

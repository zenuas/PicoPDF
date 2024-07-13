using Mina.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CharsetsExpertSubset : ICharsets
{
    public required byte Format { get; init; }
    public required ushort[] Glyph { get; init; }

    public static CharsetsExpertSubset ReadFrom(Stream stream, int glyph_count)
    {
        var glyph = new List<ushort>();
        while (glyph.Count < glyph_count)
        {
            var first = stream.ReadUShortByBigEndian();
            var left = stream.ReadUShortByBigEndian();
            Enumerable.Range(0, left + 1).Each(x => glyph.Add((ushort)(first + x)));
        }

        return new()
        {
            Format = 2,
            Glyph = [.. glyph],
        };
    }
}

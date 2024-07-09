using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CharsetsISOAdobe : ICharsets
{
    public required byte Format { get; init; }
    public required ushort[] Glyph { get; init; }

    public static CharsetsISOAdobe ReadFrom(Stream stream, int glyph_count) => new()
    {
        Format = 0,
        Glyph = Enumerable.Repeat(0, glyph_count).Select(_ => stream.ReadUShortByBigEndian()).ToArray(),
    };
}

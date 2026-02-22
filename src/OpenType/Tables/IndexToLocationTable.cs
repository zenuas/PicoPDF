using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class IndexToLocationTable
{
    public required uint[] Offsets { get; init; }

    public static IndexToLocationTable ReadFrom(Stream stream, short index_to_locformat, ushort number_of_glyphs) => new()
    {
        Offsets = index_to_locformat == 0
            ? [.. Lists.Repeat(() => (uint)stream.ReadUShortByBigEndian() * 2).Take(number_of_glyphs + 1)]
            : [.. Lists.Repeat(() => stream.ReadUIntByBigEndian()).Take(number_of_glyphs + 1)],
    };
}

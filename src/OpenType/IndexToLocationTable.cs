using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class IndexToLocationTable
{
    public required uint[] Offsets { get; init; }

    public static IndexToLocationTable ReadFrom(Stream stream, short index_to_locformat, ushort number_of_glyphs) => new()
    {
        Offsets = index_to_locformat == 0
            ? Lists.RangeTo(0, number_of_glyphs + 1).Select(_ => ((uint)BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))) * 2).ToArray()
            : Lists.RangeTo(0, number_of_glyphs + 1).Select(_ => BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4))).ToArray(),
    };
}

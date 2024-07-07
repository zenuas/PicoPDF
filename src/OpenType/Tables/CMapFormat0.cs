using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class CMapFormat0 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required byte[] GlyphIdArray { get; init; }

    public static CMapFormat0 ReadFrom(Stream stream) => new()
    {
        Format = 0,
        Length = stream.ReadUShortByBigEndian(),
        Language = stream.ReadUShortByBigEndian(),
        GlyphIdArray = Enumerable.Range(0, 256).Select(_ => stream.ReadUByte()).ToArray(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Length);
        stream.WriteUShortByBigEndian(Language);
        GlyphIdArray.Each(stream.WriteByte);
    }
}

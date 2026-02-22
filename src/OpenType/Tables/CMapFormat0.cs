using Mina.Extension;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
        GlyphIdArray = [.. Lists.Repeat(stream.ReadUByte).Take(256)],
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Length);
        stream.WriteUShortByBigEndian(Language);
        stream.Write(GlyphIdArray);
    }

    public Func<int, uint> CreateCharToGID() => (c) => GlyphIdArray[Encoding.ASCII.GetBytes([(char)c])[0]];
}

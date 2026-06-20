using Mina.Extension;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType.Tables.CMap;

public class CMapFormat0 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required byte[] GlyphIdArray { get; init; }

    public static CMapFormat0 ReadFrom(Stream stream)
    {
        var length = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = (ushort)CMapFormats.Format0,
            Length = length,
            Language = stream.ReadUShortByBigEndian(),
            GlyphIdArray = [.. Lists.Repeat(stream.ReadUByte).Take(length - /* sizeof(Format) + sizeof(Length) + sizeof(Language) */6)],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Length);
        stream.WriteUShortByBigEndian(Language);
        stream.Write(GlyphIdArray);
    }

    public static CMapFormat0 CreateFormat((char Char, byte GID)[] char_gids)
    {
        var gidarray = new byte[char_gids.Select(x => x.Char).Max() + 1];
        char_gids.Each(x => gidarray[x.Char] = x.GID);

        return new()
        {
            Format = 0,
            Length = (ushort)(gidarray.Length + /* sizeof(Format) + sizeof(Length) + sizeof(Language) */6),
            Language = 0,
            GlyphIdArray = gidarray,
        };
    }

    public Func<int, uint> CreateCharToGID() => (c) => GlyphIdArray[Encoding.ASCII.GetBytes([(char)c])[0]];
}

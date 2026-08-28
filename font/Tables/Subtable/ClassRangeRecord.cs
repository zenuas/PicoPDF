using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class ClassRangeRecord
{
    public required ushort StartGlyphID { get; init; }
    public required ushort EndGlyphID { get; init; }
    public required ushort Class { get; init; }

    public static ClassRangeRecord ReadFrom(Stream stream) => new()
    {
        StartGlyphID = stream.ReadUShortByBigEndian(),
        EndGlyphID = stream.ReadUShortByBigEndian(),
        Class = stream.ReadUShortByBigEndian(),
    };
}

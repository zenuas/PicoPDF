using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public interface ICoverageFormat
{
    public int? FindOrNull(uint gid);

    public static ICoverageFormat ReadFrom(Stream stream)
    {
        var coverage_format = stream.ReadUShortByBigEndian();
        return coverage_format switch
        {
            1 => CoverageFormat1.ReadFrom(stream),
            2 => CoverageFormat2.ReadFrom(stream),
            _ => throw new(),
        };
    }
}

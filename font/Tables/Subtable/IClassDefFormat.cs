using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public interface IClassDefFormat
{
    public ushort? GetClassValue(uint gid);

    public static IClassDefFormat ReadFrom(Stream stream)
    {
        var coverage_format = stream.ReadUShortByBigEndian();
        return coverage_format switch
        {
            1 => ClassDefFormat1.ReadFrom(stream),
            2 => ClassDefFormat2.ReadFrom(stream),
            _ => throw new(),
        };
    }
}

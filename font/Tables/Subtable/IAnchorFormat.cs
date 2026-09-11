using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public interface IAnchorFormat
{
    public static IAnchorFormat ReadFrom(Stream stream)
    {
        var anchor_format = stream.ReadUShortByBigEndian();
        return anchor_format switch
        {
            1 => AnchorFormat1.ReadFrom(stream),
            2 => AnchorFormat2.ReadFrom(stream),
            3 => AnchorFormat3.ReadFrom(stream),
            _ => throw new(),
        };
    }
}

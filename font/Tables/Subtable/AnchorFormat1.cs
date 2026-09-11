using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class AnchorFormat1 : ISubtable, IAnchorFormat
{
    public required ushort Format { get; init; }
    public required short XCoordinate { get; init; }
    public required short YCoordinate { get; init; }

    public static AnchorFormat1 ReadFrom(Stream stream) => new()
    {
        Format = 1,
        XCoordinate = stream.ReadShortByBigEndian(),
        YCoordinate = stream.ReadShortByBigEndian(),
    };
}

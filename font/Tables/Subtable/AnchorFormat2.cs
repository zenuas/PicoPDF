using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class AnchorFormat2 : ISubtable, IAnchorFormat
{
    public required ushort Format { get; init; }
    public required short XCoordinate { get; init; }
    public required short YCoordinate { get; init; }
    public required ushort AnchorPoint { get; init; }

    public static AnchorFormat2 ReadFrom(Stream stream) => new()
    {
        Format = 2,
        XCoordinate = stream.ReadShortByBigEndian(),
        YCoordinate = stream.ReadShortByBigEndian(),
        AnchorPoint = stream.ReadUShortByBigEndian(),
    };
}

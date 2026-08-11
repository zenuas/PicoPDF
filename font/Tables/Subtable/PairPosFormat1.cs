using System.IO;

namespace OpenType.Tables.Subtable;

public class PairPosFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static PairPosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

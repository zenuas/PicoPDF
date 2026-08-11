using System.IO;

namespace OpenType.Tables.Subtable;

public class PairPosFormat2 : ISubtable
{
    public required ushort Format { get; init; }

    public static PairPosFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 2,
        };
    }
}

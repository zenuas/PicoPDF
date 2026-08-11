using System.IO;

namespace OpenType.Tables.Subtable;

public class CursivePosFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static CursivePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

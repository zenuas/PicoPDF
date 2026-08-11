using System.IO;

namespace OpenType.Tables.Subtable;

public class MarkBasePosFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static MarkBasePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

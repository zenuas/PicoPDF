using System.IO;

namespace OpenType.Tables.Subtable;

public class MarkLigPosFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static MarkLigPosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

using System.IO;

namespace OpenType.Tables.Subtable;

public class SinglePosFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static SinglePosFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

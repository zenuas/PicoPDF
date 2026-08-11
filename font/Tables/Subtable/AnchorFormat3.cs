using System.IO;

namespace OpenType.Tables.Subtable;

public class AnchorFormat3 : ISubtable
{
    public required ushort Format { get; init; }

    public static AnchorFormat3 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 3,
        };
    }
}

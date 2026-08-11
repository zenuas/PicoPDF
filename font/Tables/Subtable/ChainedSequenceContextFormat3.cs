using System.IO;

namespace OpenType.Tables.Subtable;

public class ChainedSequenceContextFormat3 : ISubtable
{
    public required ushort Format { get; init; }

    public static ChainedSequenceContextFormat3 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 3,
        };
    }
}

using System.IO;

namespace OpenType.Tables.Subtable;

public class ChainedSequenceContextFormat2 : ISubtable
{
    public required ushort Format { get; init; }

    public static ChainedSequenceContextFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 2,
        };
    }
}

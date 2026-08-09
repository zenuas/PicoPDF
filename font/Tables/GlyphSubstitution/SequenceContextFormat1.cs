using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class SequenceContextFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static SequenceContextFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

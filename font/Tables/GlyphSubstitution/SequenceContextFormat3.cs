using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class SequenceContextFormat3 : ISubtable
{
    public required ushort Format { get; init; }

    public static SequenceContextFormat3 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 3,
        };
    }
}

using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class AlternateSubstFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static AlternateSubstFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

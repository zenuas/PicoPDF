using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class SingleSubstFormat2 : ISubtable
{
    public required ushort Format { get; init; }

    public static SingleSubstFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 2,
        };
    }
}

using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class SubstExtensionFormat1 : ISubtable
{
    public required ushort Format { get; init; }

    public static SubstExtensionFormat1 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 1,
        };
    }
}

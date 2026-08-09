using System.IO;

namespace OpenType.Tables.GlyphSubstitution;

public class CoverageFormat2 : ISubtable
{
    public required ushort Format { get; init; }

    public static CoverageFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        return new()
        {
            Format = 2,
        };
    }
}

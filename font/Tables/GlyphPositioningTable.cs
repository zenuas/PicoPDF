using System.IO;

namespace OpenType.Tables;

public record class GlyphPositioningTable : IExportable
{
    public static GlyphPositioningTable ReadFrom(Stream stream) => new()
    {
    };

    public void WriteTo(Stream stream)
    {
    }
}

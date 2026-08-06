using System.IO;

namespace OpenType.Tables;

public record class GlyphDefinitionTable : IExportable
{
    public static GlyphDefinitionTable ReadFrom(Stream stream) => new()
    {
    };

    public void WriteTo(Stream stream)
    {
    }
}

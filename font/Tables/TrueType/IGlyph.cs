using OpenType.Outline;
using System.Collections.Generic;

namespace OpenType.Tables.TrueType;

public interface IGlyph : IExportable
{
    public short NumberOfContours { get; init; }
    public short XMin { get; init; }
    public short YMin { get; init; }
    public short XMax { get; init; }
    public short YMax { get; init; }

    public IOutline[] ToOutline(IReadOnlyList<IGlyph> glyphs);
}

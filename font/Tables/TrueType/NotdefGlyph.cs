using OpenType.Outline;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.TrueType;

public class NotdefGlyph : IGlyph
{
    public short NumberOfContours { get; init; }
    public short XMin { get; init; }
    public short YMin { get; init; }
    public short XMax { get; init; }
    public short YMax { get; init; }

    public IOutline[] ToOutline(IReadOnlyList<IGlyph> _) => [];

    public void WriteTo(Stream stream)
    {
    }
}

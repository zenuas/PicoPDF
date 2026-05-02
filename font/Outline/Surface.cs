using System.Drawing;

namespace OpenType.Outline;

public class Surface : IOutline
{
    public required IEdge[] Edges { get; init; }
    public Color? Color { get; init; } = null;
}

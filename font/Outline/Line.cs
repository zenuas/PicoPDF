using System.Drawing;

namespace OpenType.Outline;

public class Line : IEdge
{
    public required Point Start { get; init; }
    public required Point End { get; init; }

    public override string ToString() => $"({Start.X}, {Start.Y}) -> ({End.X}, {End.Y})";
}

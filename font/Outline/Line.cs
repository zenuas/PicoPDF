using System.Numerics;

namespace OpenType.Outline;

public class Line : IEdge
{
    public required Vector2 Start { get; init; }
    public required Vector2 End { get; init; }

    public override string ToString() => $"({Start.X}, {Start.Y}) -> ({End.X}, {End.Y})";
}

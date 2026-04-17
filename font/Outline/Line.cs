using System;
using System.Numerics;

namespace OpenType.Outline;

public class Line : IEdge
{
    public required Vector2 Start { get; init; }
    public required Vector2 End { get; init; }

    public float MinX() => Math.Min(Start.X, End.X);
    public float MinY() => Math.Min(Start.Y, End.Y);
    public float MaxX() => Math.Max(Start.X, End.X);
    public float MaxY() => Math.Max(Start.Y, End.Y);

    public override string ToString() => $"({Start.X}, {Start.Y}) -> ({End.X}, {End.Y})";
}

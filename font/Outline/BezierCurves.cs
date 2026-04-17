using Mina.Extension;
using System;
using System.Linq;
using System.Numerics;

namespace OpenType.Outline;

public class BezierCurves : IEdge
{
    public required Vector2 Start { get; init; }
    public required Vector2 End { get; init; }
    public required Vector2[] ControlPoint { get; init; }
    public required bool ComplementPoint { get; init; }

    public float MinX() => Math.Min(Math.Min(Start.X, End.X), ControlPoint.Select(x => x.X).Min());
    public float MinY() => Math.Min(Math.Min(Start.Y, End.Y), ControlPoint.Select(x => x.Y).Min());
    public float MaxX() => Math.Max(Math.Max(Start.X, End.X), ControlPoint.Select(x => x.X).Max());
    public float MaxY() => Math.Max(Math.Max(Start.Y, End.Y), ControlPoint.Select(x => x.Y).Max());

    public override string ToString() => $"({Start.X}, {Start.Y}) -> {ControlPoint.Select(p => $"({p.X}, {p.Y})").Join(" -> ")} -> ({End.X}, {End.Y})";
}

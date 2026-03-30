using Mina.Extension;
using System.Linq;
using System.Numerics;

namespace OpenType.Outline;

public class BezierCurves : IEdge
{
    public required Vector2 Start { get; init; }
    public required Vector2 End { get; init; }
    public required Vector2[] ControlPoint { get; init; }

    public override string ToString() => $"({Start.X}, {Start.Y}) -> {ControlPoint.Select(p => $"({p.X}, {p.Y})").Join(" -> ")} -> ({End.X}, {End.Y})";
}

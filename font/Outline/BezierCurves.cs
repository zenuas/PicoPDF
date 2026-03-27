using Mina.Extension;
using System.Drawing;
using System.Linq;

namespace OpenType.Outline;

public class BezierCurves : IEdge
{
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required Point[] ControlPoint { get; init; }

    public override string ToString() => $"({Start.X}, {Start.Y}) -> {ControlPoint.Select(p => $"({p.X}, {p.Y})").Join(" -> ")} -> ({End.X}, {End.Y})";
}

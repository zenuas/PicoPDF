using System.Collections.Generic;
using System.Numerics;

namespace OpenType.Outline;

public class Surface
{
    public required IEdge[] Edges { get; init; }

    public IEnumerable<Vector2> GetPoints()
    {
        foreach (var edge in Edges)
        {
            yield return edge.Start;

            if (edge is BezierCurve bezier)
            {
                foreach (var cp in bezier.ControlPoint) yield return cp;
            }

            yield return edge.End;
        }
    }
}

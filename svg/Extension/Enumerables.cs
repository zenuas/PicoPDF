using Mina.Extension;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Svg.Extension;

public static class Enumerables
{
    public static IEnumerable<Surface> GetSurfaces(this IEnumerable<IOutline> outlines)
    {
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface:
                    yield return surface;
                    break;

                case Layer layer:
                    foreach (var x in layer.Surfaces.GetSurfaces()) yield return x;
                    break;
            }
        }
    }

    public static (float Width, float Left, float YMax, float YMin) GetSurfaceSize(this IEnumerable<Surface> surfaces) => surfaces.Select(x => x.Edges).Flatten().GetEdgesSize();

    public static (float Width, float Left, float YMax, float YMin) GetEdgesSize(this IEnumerable<IEdge> edges)
    {
        var points = edges.GetPoints();
        if (points.IsEmpty()) return (0, 0, 0, 0);

        var first = points.First();
        var xmin = first.X;
        var ymin = first.Y;
        var xmax = first.X;
        var ymax = first.Y;
        foreach (var point in points.Skip(1))
        {
            xmin = Math.Min(xmin, point.X);
            ymin = Math.Min(ymin, point.Y);
            xmax = Math.Max(xmax, point.X);
            ymax = Math.Max(ymax, point.Y);
        }
        return (xmax - xmin, xmin, ymax, ymin);
    }

    public static IEnumerable<Vector2> GetPoints(this IEnumerable<IEdge> edges)
    {
        foreach (var edge in edges)
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

using Mina.Extension;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Svg;

public static class SvgUtility
{
    public static IEnumerable<Surface> GetSurfaces(IEnumerable<IOutline> outlines)
    {
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface:
                    yield return surface;
                    break;

                case Layer layer:
                    foreach (var x in GetSurfaces(layer.Surfaces)) yield return x;
                    break;
            }
        }
    }

    public static (float Width, float Left, float YMax, float YMin) GetSurfaceSize(IEnumerable<Surface> surfaces) => GetEdgesSize(surfaces.Select(x => x.Edges).Flatten());

    public static (float Width, float Left, float YMax, float YMin) GetEdgesSize(IEnumerable<IEdge> edges)
    {
        var points = GetPoints(edges);
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

    public static IEnumerable<Vector2> GetPoints(IEnumerable<IEdge> edges)
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

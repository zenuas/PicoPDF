using Mina.Extension;
using OpenType.Outline;
using OpenType.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenType;

public static class Utility
{
    public static bool ContainTrueType(this OffsetTable self) => self.SfntVersion == 0x00010000 || self.SfntVersion == 0x74727565;

    public static bool ContainCFF(this OffsetTable self) => self.SfntVersion == 0x4F54544F;

    public static double MeasureString(this IOpenTypeFont font, string s) => s.ToUtf32CharArray().Select(x => MeasureChar(font, x)).Sum();

    public static double MeasureChar(this IOpenTypeFont font, int c) => MeasureGID(font, font.CharToGID(c));

    public static double MeasureGID(this IOpenTypeFont font, uint gid) => (double)font.GetAdvanceWidth(gid) / font.FontHeader.UnitsPerEm;

    // If numberOfHMetrics is less than the total number of glyphs,
    // then the hMetrics array is followed by an array for the left side bearing values of the remaining glyphs.
    public static int GetAdvanceWidth(this IOpenTypeFont font, uint gid) => font.HorizontalMetrics.Metrics[Math.Min(gid, font.HorizontalHeader.NumberOfHMetrics - 1)].AdvanceWidth;

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

                default:
                    throw new();
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
